using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using courseProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace courseProject.Data
{
    public class ServicesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(AppDbContext context, ILogger<ServicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Services
        public async Task<IActionResult> Index()
        {
            try
            {
                var appDbContext = _context.Services.Include(s => s.Category);
                ViewData["CategoryId"] = new SelectList(_context.ServiceCategories, "CategoryId", "CategoryTitle");
                return View(await appDbContext.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load services from DB");
                ViewData["DbError"] = "Ошибка подключения к базе данных. Услуги недоступны.";
                ViewData["CategoryId"] = new SelectList(Enumerable.Empty<SelectListItem>());
                return View(Enumerable.Empty<courseProject.Models.Service>());
            }
        }

        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewData["CategoryId"] = new SelectList(_context.ServiceCategories, "CategoryId", "CategoryTitle");

            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.ServicesId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // GET: Services/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.ServiceCategories, "CategoryId", "CategoryTitle");
            return View();
        }

        // POST: Services/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("ServicesId,ServicesTitle,CategoryId,Price,Description")] Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.ServiceCategories, "CategoryId", "CategoryTitle");

            return View(service);
        }

        // GET: Services/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.ServiceCategories, "CategoryId", "CategoryTitle");
            return View(service);
        }

        // POST: Services/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ServicesId,ServicesTitle,CategoryId,Price,Description")] Service service)
        {
            if (id != service.ServicesId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServicesId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.ServiceCategories, "CategoryId", "CategoryTitle");
            return View(service);
        }

        // GET: Services/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.ServicesId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Новый: получить список услуг в JSON (для клиентских форм)
        [HttpGet]
        public async Task<IActionResult> ListJson()
        {
            var list = await _context.Services.Include(s => s.Category)
                .Select(s => new {
                    id = s.ServicesId,
                    title = s.ServicesTitle,
                    price = s.Price,
                    description = s.Description,
                    category = s.Category != null ? s.Category.CategoryTitle : null
                }).ToListAsync();

            return Json(list);
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServicesId == id);
        }
    }
}
