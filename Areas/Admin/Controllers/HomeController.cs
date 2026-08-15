using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class HomeController : Controller
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public HomeController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        public async Task<IActionResult> Index(DateTime? date, CancellationToken ct)
        {
            var model = await _adminDashboardService.GetUnionDashboardAsync(date, ct);
            return View(model);
        }
    }
}
