using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Controllers
{
    // Deliberately minimal for Stage 1. Once Stage 3 (Identity) exists, this
    // becomes the public landing page that redirects authenticated users to
    // their role-specific Area (Admin/Operator/Farmer) dashboard.
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
