using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Farmer.Controllers
{
    [Area("Farmer")]
    [Authorize(Roles = Roles.Farmer)]
    public class HomeController : Controller
    {
        private readonly IFarmerService _farmerService;
        private readonly IMilkCollectionService _collectionService;
        private readonly IPaymentService _paymentService;
        private readonly IFeedIssueService _feedIssueService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IFarmerService farmerService,
            IMilkCollectionService collectionService,
            IPaymentService paymentService,
            IFeedIssueService feedIssueService,
            UserManager<ApplicationUser> userManager)
        {
            _farmerService = farmerService;
            _collectionService = collectionService;
            _paymentService = paymentService;
            _feedIssueService = feedIssueService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var farmer = await CurrentFarmerAsync(ct);
            if (farmer is null)
            {
                return View("NoProfile");
            }

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var todayCollections = await _collectionService.GetByFarmerAndDateRangeAsync(farmer.FarmerID, today, today, ct);
            var monthCollections = await _collectionService.GetByFarmerAndDateRangeAsync(farmer.FarmerID, monthStart, today, ct);
            var settlements = await _paymentService.GetByFarmerAsync(farmer.FarmerID, ct);
            var latestSettlement = settlements.FirstOrDefault(p => p.Status != SettlementStatus.Draft);

            var farmerIssues = await _feedIssueService.GetByFarmerAsync(farmer.FarmerID, farmer.SocietyID, ct);
            var pendingIssues = farmerIssues.Where(i => !i.IsLocked).ToList();

            var model = new FarmerDashboardViewModel
            {
                FarmerCode = farmer.FarmerCode,
                FullName = farmer.FullName,
                TodayQuantity = todayCollections.Sum(c => c.Quantity),
                TodayAmount = todayCollections.Sum(c => c.Amount),
                MonthlyQuantity = monthCollections.Sum(c => c.Quantity),
                MonthlyAverageFat = monthCollections.Count > 0 ? monthCollections.Average(c => c.FatPercent) : 0,
                HasLatestSettlement = latestSettlement is not null,
                LatestSettlementPeriodStart = latestSettlement?.PeriodStart ?? default,
                LatestSettlementPeriodEnd = latestSettlement?.PeriodEnd ?? default,
                LatestSettlementNetAmount = latestSettlement?.NetAmount ?? 0,
                LatestSettlementStatus = latestSettlement?.Status ?? SettlementStatus.Draft,
                PendingFeedDeduction = pendingIssues.Where(i => i.ItemType == ItemType.Feed).Sum(i => i.TotalCost),
                PendingMedicineDeduction = pendingIssues.Where(i => i.ItemType == ItemType.Medicine).Sum(i => i.TotalCost)
            };

            return View(model);
        }

        private async Task<Models.Entities.Farmer?> CurrentFarmerAsync(CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return null;
            return await _farmerService.GetByUserIdAsync(user.Id, ct);
        }
    }
}
