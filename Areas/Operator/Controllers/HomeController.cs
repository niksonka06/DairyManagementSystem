using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DairyManagementSystem.Areas.Operator.Controllers
{
    public class HomeController : OperatorControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IFarmerRepository _farmerRepository;
        private readonly IMilkCollectionRepository _collectionRepository;
        private readonly IPaymentRepository _paymentRepository;

        public HomeController(
            IReportService reportService,
            IFarmerRepository farmerRepository,
            IMilkCollectionRepository collectionRepository,
            IPaymentRepository paymentRepository,
            UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _reportService = reportService;
            _farmerRepository = farmerRepository;
            _collectionRepository = collectionRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<IActionResult> Index(DateTime? date, CancellationToken ct)
        {
            var societyId = await CurrentOperatorSocietyIdAsync();
            var overviewDate = (date ?? DateTime.Today).Date;

            var daily = await _reportService.GetDailyCollectionReportAsync(societyId, overviewDate, ct);
            var farmers = await _farmerRepository.GetBySocietyAsync(societyId, ct);
            var collections = await _collectionRepository.GetBySocietyAndDateAsync(societyId, overviewDate, ct);
            var payments = await _paymentRepository.GetBySocietyAsync(societyId, ct);

            var trendLabels = new List<string>();
            var trendValues = new List<decimal>();
            foreach (var day in Enumerable.Range(0, 7).Select(i => overviewDate.AddDays(-i)).OrderBy(d => d))
            {
                var report = await _reportService.GetDailyCollectionReportAsync(societyId, day, ct);
                trendLabels.Add(day.ToString("dd-MMM"));
                trendValues.Add(report.TotalQuantity);
            }

            var generated = payments.Where(p => p.Status == SettlementStatus.Generated);

            var model = new OperatorDashboardViewModel
            {
                OverviewDate = overviewDate,
                TodayMilkLitres = daily.TotalQuantity,
                TodayPayable = daily.TotalAmount,
                ActiveFarmerCount = farmers.Count(f => f.IsActive),
                PendingSettlementsAmount = SettlementKpis.PayableGenerated(generated),
                CarryForwardAmount = SettlementKpis.CarryForwardGenerated(generated),
                MorningEntries = collections.Count(c => c.Shift == Shift.Morning),
                EveningEntries = collections.Count(c => c.Shift == Shift.Evening),
                TrendLabels = trendLabels,
                TrendValues = trendValues
            };

            return View(model);
        }
    }
}
