using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using courseProject.Data;
using Microsoft.EntityFrameworkCore;
using courseProject.Models;
using System.Security.Claims;

namespace courseProject.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Services.AuthService _authService;
        private readonly IWebHostEnvironment _env;

        public AccountController(AppDbContext context, Services.AuthService authService, IWebHostEnvironment env)
        {
            _context = context;
            _authService = authService;
            _env = env;
        }

        // GET: /Account/Profile
        public IActionResult Profile()
        {
            var idClaim = User.FindFirst("id")?.Value;
            if (!int.TryParse(idClaim, out var userId))
            {
                return Forbid();
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return NotFound();

            // get user's requests by email (from DB)
            List<RequestModel> reqs;
            try
            {
                reqs = _context.RequestEntities
                    .Include(r => r.Items)
                    .Where(r => r.CustomerEmail == user.Email)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList()
                    .Select(r => new RequestModel {
                        Id = r.RequestEntityId,
                        CustomerName = r.CustomerName,
                        CustomerEmail = r.CustomerEmail,
                        CustomerPhone = r.CustomerPhone,
                        Items = r.Items.Select(i => new RequestItem { Title = i.Title, Price = i.Price }).ToList(),
                        Total = r.Total,
                        Status = r.Status,
                        AssignedTo = r.AssignedTo,
                        CreatedAt = r.CreatedAt
                    }).ToList();
            }
            catch (System.Exception)
            {
                // If the DB table doesn't exist yet (migration not applied), don't crash the profile page.
                reqs = new List<RequestModel>();
            }

            var vm = new ProfileViewModel
            {
                User = user,
                Requests = reqs
            };

            return View(vm);
        }

        // GET: /Account/Edit
        public IActionResult Edit()
        {
            var idClaim = User.FindFirst("id")?.Value;
            if (!int.TryParse(idClaim, out var userId))
            {
                return Forbid();
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /Account/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("FullName,Phone")] User model, IFormFile? avatarFile, string? currentPassword, string? newPassword, string? confirmPassword)
        {
            var idClaim = User.FindFirst("id")?.Value;
            if (!int.TryParse(idClaim, out var userId))
            {
                return Forbid();
            }
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return NotFound();

            // Update basic fields
            user.FullName = model.FullName ?? user.FullName;
            user.Phone = model.Phone ?? user.Phone;

            // Avatar upload
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                var ext = Path.GetExtension(avatarFile.FileName);
                var fileName = $"avatar_{user.UserId}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await avatarFile.CopyToAsync(stream);
                }
                user.AvatarPath = "/uploads/avatars/" + fileName;
            }

            // Change password if requested
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "Новый пароль и подтверждение не совпадают");
                    return View(user);
                }

                if (string.IsNullOrEmpty(currentPassword) || !_authService.ChangePassword(user, currentPassword, newPassword))
                {
                    ModelState.AddModelError("", "Текущий пароль неверен или не указан");
                    return View(user);
                }
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Profile));
        }
    }
}
