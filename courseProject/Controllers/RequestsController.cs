using Microsoft.AspNetCore.Mvc;
using courseProject.Data;
using courseProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Text;
using ClosedXML.Excel;

namespace courseProject.Controllers
{
    public class RequestsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RequestsController> _logger;

        // In-memory storage for demo — публичный, чтобы профиль мог получить свои заявки
        public static readonly List<RequestModel> Requests = new();
        private static int _nextId = 1;

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
            return View();
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
        [Authorize]
        public IActionResult Submit([FromBody] RequestSubmitDto dto)
        {
            if (dto == null || dto.Items == null || !dto.Items.Any())
                return BadRequest(new { success = false, message = "Пустая заявка" });

            var userEmailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                                 ?? User.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(userEmailClaim))
            {
                return Unauthorized(new { success = false, message = "Не удалось определить пользователя" });
            }

            var model = new RequestModel
            {
                Id = _nextId++,
                CustomerName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? dto.CustomerName,
                CustomerEmail = userEmailClaim,
                CustomerPhone = dto.CustomerPhone,
                Items = dto.Items.Select(i => new RequestItem { ServiceId = i.ServiceId, Title = i.Title, Price = i.Price }).ToList(),
                Total = dto.Items.Sum(i => i.Price),
                Status = "Новая",
                CreatedAt = DateTime.UtcNow
            };

            Requests.Add(model);

            return Ok(new { success = true, id = model.Id });
        }

        // API: получить все заявки (админ)
        [HttpGet]
        [Route("api/requests")]
        [Authorize(Roles = "admin")]
        public IActionResult List()
        {
            return Json(Requests);
        }

        // API: изменить статус заявки (админ)
        [HttpPost]
        [Route("api/requests/{id}/status")]
        [Authorize(Roles = "admin")]
        public IActionResult ChangeStatus(int id, [FromBody] ChangeStatusDto dto)
        {
            var r = Requests.FirstOrDefault(x => x.Id == id);
            if (r == null) return NotFound();
            r.Status = dto.Status ?? r.Status;
            r.AssignedTo = dto.AssignedTo ?? r.AssignedTo;
            return Ok(new { success = true });
        }

        // Export CSV (kept for compatibility) - admin only
        [Authorize(Roles = "admin")]
        public IActionResult Export()
        {
            // Build CSV with UTF-8 BOM so Excel opens it correctly
            static string Escape(string s) => (s ?? "").Replace("\"", "\"\"");

            var sb = new StringBuilder();
            // header
            sb.AppendLine("\"Id\",\"CustomerName\",\"CustomerEmail\",\"CustomerPhone\",\"Items\",\"Total\",\"Status\",\"AssignedTo\",\"CreatedAt\"");

            foreach (var r in Requests)
            {
                var itemsText = string.Join(" | ", (r.Items ?? new System.Collections.Generic.List<RequestItem>()).Select(i => $"{i.Title} ({i.Price})"));
                sb.AppendLine(
                    $"\"{Escape(r.Id.ToString())}\",\"{Escape(r.CustomerName)}\",\"{Escape(r.CustomerEmail)}\",\"{Escape(r.CustomerPhone ?? "")}\",\"{Escape(itemsText)}\",\"{Escape(r.Total.ToString())}\",\"{Escape(r.Status)}\",\"{Escape(r.AssignedTo ?? "")}\",\"{Escape(r.CreatedAt.ToString("u"))}\""
                );
            }

            var bytes = new UTF8Encoding(true).GetBytes(sb.ToString()); // BOM
            return File(bytes, "text/csv; charset=utf-8", $"requests_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        // New: Export to Excel (.xlsx) using ClosedXML
        [Authorize(Roles = "admin")]
        public IActionResult ExportExcel()
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
            foreach (var r in Requests)
            {
                var itemsText = string.Join(" | ", r.Items.Select(i => $"{i.Title} ({i.Price})"));
                ws.Cell(row, 1).Value = r.Id;
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
