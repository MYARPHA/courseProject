using Microsoft.AspNetCore.Mvc;
using courseProject.Data;
using courseProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using System.Text;
using ClosedXML.Excel;

namespace courseProject.Controllers
{
    public class RequestsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RequestsController> _logger;

        // Note: persist requests in DB using RequestEntity / RequestItemEntity

        public RequestsController(AppDbContext context, ILogger<RequestsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Requests/Create
        // Разрешаем открывать страницу формы любому пользователю — проверку авторизации делаем на клиенте и на POST-запросе
        public IActionResult Create()
        {
            return View();
        }

        // GET: Admin view
        public IActionResult Index() => RedirectToAction("Admin");

        public IActionResult Admin()
        {
            // redirect to centralized admin UI
            return RedirectToAction("Requests", "Admin");
        }

        // API: status definitions (try to read from DB table `request_statuses`, fallback to builtin list)
        [HttpGet]
        [Route("api/requests/statuses/definitions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin")]
        public IActionResult StatusDefinitions()
        {
            try
            {
                var conn = _context.Database.GetDbConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT name, color, icon FROM request_statuses";
                var list = new System.Collections.Generic.List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var name = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    var color = reader.FieldCount > 1 && !reader.IsDBNull(1) ? reader.GetString(1) : null;
                    var icon = reader.FieldCount > 2 && !reader.IsDBNull(2) ? reader.GetString(2) : null;
                    list.Add(new { name, color, icon });
                }
                conn.Close();
                if (list.Count > 0) return Json(list);
            }
            catch (System.Exception ex)
            {
                _logger.LogDebug(ex, "Could not read request_statuses table");
            }

            // fallback definitions
            var fallback = new[] {
                new { name = "Новая", color = "#6c757d", icon = "🟣" },
                new { name = "В обработке", color = "#fd7e14", icon = "🟡" },
                new { name = "Завершена", color = "#28a745", icon = "🟢" },
                new { name = "Отменена", color = "#dc3545", icon = "🔴" }
            };
            return Json(fallback);
        }

        // API: получить список услуг (для формы заявок)
        [HttpGet]
        [Route("api/requests/services")]
        public async Task<IActionResult> Services()
        {
            var list = await _context.Services
                .Select(s => new {
                    id = s.ServicesId,
                    title = s.ServicesTitle,
                    price = s.Price,
                    description = s.Description
                })
                .ToListAsync();

            return Json(list);
        }

        // API: отправка заявки (только авторизованные)
        [HttpPost]
        [Route("api/requests/submit")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Submit([FromBody] RequestSubmitDto dto)
        {
            if (dto == null || dto.Items == null || !dto.Items.Any())
            {
                _logger.LogWarning("Submit called with empty dto by {User}", User?.Identity?.Name ?? "unknown");
                return BadRequest(new { success = false, message = "Пустая заявка" });
            }

            var userEmailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                                 ?? User.FindFirst("email")?.Value;

            _logger.LogInformation("Submit called by {email} with {count} items", userEmailClaim ?? "(no-email)", dto.Items.Count);

            if (string.IsNullOrEmpty(userEmailClaim))
            {
                return Unauthorized(new { success = false, message = "Не удалось определить пользователя" });
            }

            var nameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? dto.CustomerName;

            var entity = new RequestEntity
            {
                CustomerName = nameClaim,
                CustomerEmail = userEmailClaim,
                CustomerPhone = dto.CustomerPhone,
                Status = "Новая",
                Total = dto.Items.Sum(i => i.Price),
                CreatedAt = DateTime.UtcNow
            };

            foreach (var it in dto.Items)
            {
                entity.Items.Add(new RequestItemEntity { Title = it.Title, Price = it.Price });
            }

            _context.RequestEntities.Add(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                _logger.LogWarning(dbEx, "SaveChanges failed for RequestEntity - attempting EnsureCreated and retry");
                try
                {
                    // Try to create missing tables (best-effort). If migrations are used, prefer applying migrations instead.
                    _context.Database.EnsureCreated();
                    await _context.SaveChangesAsync();
                }
                catch (Exception retryEx)
                {
                    _logger.LogError(retryEx, "Retry SaveChanges failed after EnsureCreated");
                    return StatusCode(500, new { success = false, message = "Ошибка сохранения в БД: отсутствуют нужные таблицы или схема не настроена. Пожалуйста, примените миграции или выполните инициализацию БД." });
                }
            }

            return Ok(new { success = true, id = entity.RequestEntityId });
        }

        // API: получить все заявки (админ)
        [HttpGet]
        [Route("api/requests")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin")]
        public async Task<IActionResult> List()
        {
            try
            {
                var list = await _context.RequestEntities
                    .Include(r => r.Items)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                var dto = list.Select(r => new {
                    id = r.RequestEntityId,
                    customerName = r.CustomerName,
                    customerEmail = r.CustomerEmail,
                    customerPhone = r.CustomerPhone,
                    items = r.Items.Select(i => new { title = i.Title, price = i.Price }),
                    total = r.Total,
                    status = r.Status,
                    assignedTo = r.AssignedTo,
                    createdAt = r.CreatedAt
                });

                return Json(dto);
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Could not read RequestEntities table — returning empty list");
                return Json(new object[0]);
            }
        }

        // API: изменить статус заявки (админ)
        [HttpPost]
        [Route("api/requests/{id}/status")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto dto)
        {
            var r = await _context.RequestEntities.Include(x => x.Items).FirstOrDefaultAsync(x => x.RequestEntityId == id);
            if (r == null) return NotFound();
            r.Status = dto.Status ?? r.Status;
            r.AssignedTo = dto.AssignedTo ?? r.AssignedTo;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // Export CSV (kept for compatibility) - admin only
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin")]
        public async Task<IActionResult> Export()
        {
            // Build CSV with UTF-8 BOM so Excel opens it correctly
            static string Escape(string s) => (s ?? "").Replace("\"", "\"\"");

            var sb = new StringBuilder();
            // header
            sb.AppendLine("\"Id\",\"CustomerName\",\"CustomerEmail\",\"CustomerPhone\",\"Items\",\"Total\",\"Status\",\"AssignedTo\",\"CreatedAt\"");

            try
            {
                var rows = await _context.RequestEntities.Include(r => r.Items).OrderByDescending(r => r.CreatedAt).ToListAsync();
                foreach (var r in rows)
                {
                    var itemsText = string.Join(" | ", (r.Items ?? new System.Collections.Generic.List<RequestItemEntity>()).Select(i => $"{i.Title} ({i.Price})"));
                    sb.AppendLine(
                        $"\"{Escape(r.RequestEntityId.ToString())}\",\"{Escape(r.CustomerName)}\",\"{Escape(r.CustomerEmail)}\",\"{Escape(r.CustomerPhone ?? "")}\",\"{Escape(itemsText)}\",\"{Escape(r.Total.ToString())}\",\"{Escape(r.Status)}\",\"{Escape(r.AssignedTo ?? "")}\",\"{Escape(r.CreatedAt.ToString("u"))}\""
                    );
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Could not read RequestEntities table for export — returning header-only CSV");
                // fall through with empty rows (header only)
            }

            var bytes = new UTF8Encoding(true).GetBytes(sb.ToString()); // BOM
            return File(bytes, "text/csv; charset=utf-8", $"requests_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        // New: Export to Excel (.xlsx) using ClosedXML
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin")]
        public async Task<IActionResult> ExportExcel()
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Requests");
            // header
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "CustomerName";
            ws.Cell(1, 3).Value = "CustomerEmail";
            ws.Cell(1, 4).Value = "CustomerPhone";
            ws.Cell(1, 5).Value = "Items";
            ws.Cell(1, 6).Value = "Total";
            ws.Cell(1, 7).Value = "Status";
            ws.Cell(1, 8).Value = "AssignedTo";
            ws.Cell(1, 9).Value = "CreatedAt";

            var row = 2;
            try
            {
                var rows = await _context.RequestEntities.Include(r => r.Items).OrderByDescending(r => r.CreatedAt).ToListAsync();
                foreach (var r in rows)
                {
                    var itemsText = string.Join(" | ", (r.Items ?? new System.Collections.Generic.List<RequestItemEntity>()).Select(i => $"{i.Title} ({i.Price})"));
                    ws.Cell(row, 1).Value = r.RequestEntityId;
                    ws.Cell(row, 2).Value = r.CustomerName;
                    ws.Cell(row, 3).Value = r.CustomerEmail;
                    ws.Cell(row, 4).Value = r.CustomerPhone;
                    ws.Cell(row, 5).Value = itemsText;
                    ws.Cell(row, 6).Value = r.Total;
                    ws.Cell(row, 7).Value = r.Status;
                    ws.Cell(row, 8).Value = r.AssignedTo;
                    ws.Cell(row, 9).Value = r.CreatedAt.ToString("u");
                    row++;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Could not read RequestEntities table for Excel export — returning workbook with header only");
            }

            using var ms = new System.IO.MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;
            var fileName = $"requests_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }

    // DTOs for incoming requests (unchanged)
    public class RequestSubmitDto
    {
        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string? CustomerPhone { get; set; }
        public List<RequestItem> Items { get; set; } = new();
    }

    public class ChangeStatusDto
    {
        public string? Status { get; set; }
        public string? AssignedTo { get; set; }
    }
}
