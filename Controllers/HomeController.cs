using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(Roles.Admin))
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                if (User.IsInRole(Roles.Operator))
                    return RedirectToAction("Index", "Home", new { area = "Operator" });
                if (User.IsInRole(Roles.Farmer))
                    return RedirectToAction("Index", "Home", new { area = "Farmer" });
            }

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
