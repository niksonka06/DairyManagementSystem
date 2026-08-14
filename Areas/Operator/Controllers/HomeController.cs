using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Operator.Controllers
{
    [Area("Operator")]
    [Authorize(Roles = Roles.Operator)]
    public class HomeController : Controller
    {
        private readonly IReportService _reportService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IReportService reportService, UserManager<ApplicationUser> userManager)
        {
            _reportService = reportService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.SocietyID is int societyId)
            {
                // Last 7 days, one DailyCollectionReport call per day — fine
                // at this data scale for a dashboard widget; a dedicated
                // "trend" query would be a reasonable optimization if this
                // society's data volume grew much larger.
                var days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).OrderBy(d => d).ToList();
                var trend = new List<(DateTime Date, decimal Quantity)>();
                foreach (var day in days)
                {
                    var report = await _reportService.GetDailyCollectionReportAsync(societyId, day, ct);
                    trend.Add((day, report.TotalQuantity));
                }

                ViewBag.TrendLabels = trend.Select(t => t.Date.ToString("dd-MMM")).ToList();
                ViewBag.TrendValues = trend.Select(t => t.Quantity).ToList();
            }

            return View();
        }
    }
}
