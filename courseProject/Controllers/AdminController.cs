using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace courseProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        // GET: /Admin/Requests
        public IActionResult Requests()
        {
            return View();
        }
    }
}
