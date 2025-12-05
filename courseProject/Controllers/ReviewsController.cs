using Microsoft.AspNetCore.Mvc;
using courseProject.Data;
using courseProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace courseProject.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;
        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Reviews
        [HttpGet]
        public IActionResult Index()
        {
            var reviews = _context.Reviews.OrderByDescending(r => r.CreatedAt).ToList();
            return View(reviews);
        }

        // API: GET /api/reviews
        [HttpGet]
        [Route("api/reviews")]
        public IActionResult GetAll()
        {
            var reviews = _context.Reviews.OrderByDescending(r => r.CreatedAt).ToList();
            return Ok(reviews);
        }

        // API: POST /api/reviews
        [HttpPost]
        [Route("api/reviews")]
        [Authorize]
        public IActionResult Create([FromBody] CreateReviewDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Text)) return BadRequest(new { success = false, message = "Empty review" });
            if (dto.Rating < 1 || dto.Rating > 5) dto.Rating = 5;

            var idClaim = User.FindFirst("id")?.Value;
            if (!int.TryParse(idClaim, out var userId)) return Forbid();

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return Forbid();

            var review = new Review
            {
                AuthorId = user.UserId.ToString(),
                AuthorName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName,
                Text = dto.Text,
                Rating = dto.Rating,
                CreatedAt = System.DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            return Ok(new { success = true, review });
        }
    }

    public class CreateReviewDto
    {
        public string Text { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
    }
}
