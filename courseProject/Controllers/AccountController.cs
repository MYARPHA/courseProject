using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using courseProject.Data;
using courseProject.Models;
using System.Security.Claims;

namespace courseProject.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
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

            // get user's requests by email
            var requests = RequestsController.Requests.Where(r => r.CustomerEmail == user.Email).OrderByDescending(r => r.CreatedAt).ToList();

            var vm = new ProfileViewModel
            {
                User = user,
                Requests = requests
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
        public IActionResult Edit([Bind("FullName")] User model)
        {
            var idClaim = User.FindFirst("id")?.Value;
            if (!int.TryParse(idClaim, out var userId))
            {
                return Forbid();
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return NotFound();

            user.FullName = model.FullName ?? user.FullName;
            _context.SaveChanges();

            return RedirectToAction(nameof(Profile));
        }
    }
}
