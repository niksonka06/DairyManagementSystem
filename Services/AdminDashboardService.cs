using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ISocietyService _societyService;
        private readonly IFarmerRepository _farmerRepository;
        private readonly IUserManagementService _userManagementService;
        private readonly IReportService _reportService;
        private readonly IMilkCollectionRepository _collectionRepository;
        private readonly IPaymentRepository _paymentRepository;

        public AdminDashboardService(
            ISocietyService societyService,
            IFarmerRepository farmerRepository,
            IUserManagementService userManagementService,
            IReportService reportService,
            IMilkCollectionRepository collectionRepository,
            IPaymentRepository paymentRepository)
        {
            _societyService = societyService;
            _farmerRepository = farmerRepository;
            _userManagementService = userManagementService;
            _reportService = reportService;
            _collectionRepository = collectionRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<AdminDashboardViewModel> GetUnionDashboardAsync(DateTime? date = null, CancellationToken ct = default)
        {
            var overviewDate = (date ?? DateTime.Today).Date;
            var societies = await _societyService.GetAllAsync(ct);
            var operators = await _userManagementService.GetOperatorsAsync(ct);

            var model = new AdminDashboardViewModel
            {
                OverviewDate = overviewDate,
                TotalSocietyCount = societies.Count,
                ActiveSocietyCount = societies.Count(s => s.IsActive)
            };

            foreach (var society in societies.OrderBy(s => s.SocietyName))
            {
                var row = await BuildSocietyPerformanceRowAsync(society.SocietyID, society.SocietyName, society.IsActive, operators, overviewDate, ct);
                model.Societies.Add(row);

                model.TotalMilkTodayLitres += row.TodayMilkLitres;
                model.TotalPayableToday += row.TodayPayable;
                model.ActiveFarmerCount += row.ActiveFarmerCount;
                model.PendingSettlementsAmount += row.PendingSettlementsAmount;
                model.PendingSettlementsCount += row.PendingSettlementsCount;
            }

            return model;
        }

        public async Task<SocietyOverviewViewModel?> GetSocietyOverviewAsync(int societyId, DateTime? date = null, CancellationToken ct = default)
        {
            var society = await _societyService.GetByIdAsync(societyId, ct);
            if (society is null)
            {
                return null;
            }

            var overviewDate = (date ?? DateTime.Today).Date;
            var operators = await _userManagementService.GetOperatorsAsync(ct);
            var performance = await BuildSocietyPerformanceRowAsync(
                society.SocietyID, society.SocietyName, society.IsActive, operators, overviewDate, ct);

            var farmers = await _farmerRepository.GetBySocietyAsync(societyId, ct);
            var payments = await _paymentRepository.GetBySocietyAsync(societyId, ct);
            var pending = payments.Where(p => p.Status == SettlementStatus.Generated).ToList();

            var trendLabels = new List<string>();
            var trendValues = new List<decimal>();
            for (var i = 6; i >= 0; i--)
            {
                var day = overviewDate.AddDays(-i);
                var report = await _reportService.GetDailyCollectionReportAsync(societyId, day, ct);
                trendLabels.Add(day.ToString("dd-MMM"));
                trendValues.Add(report.TotalQuantity);
            }

            return new SocietyOverviewViewModel
            {
                SocietyID = society.SocietyID,
                SocietyName = society.SocietyName,
                RegistrationNo = society.RegistrationNo,
                Address = society.Address,
                ContactPhone = society.ContactPhone,
                IsActive = society.IsActive,
                OverviewDate = overviewDate,
                TodayMilkLitres = performance.TodayMilkLitres,
                TodayPayable = performance.TodayPayable,
                ActiveFarmerCount = performance.ActiveFarmerCount,
                TotalFarmerCount = farmers.Count,
                PendingSettlementsAmount = pending.Sum(p => p.NetAmount),
                PendingSettlementsCount = pending.Count,
                MorningEntries = performance.MorningEntries,
                EveningEntries = performance.EveningEntries,
                Operators = operators
                    .Where(o => o.SocietyID == societyId)
                    .OrderBy(o => o.FullName)
                    .Select(o => new SocietyOperatorRowViewModel
                    {
                        UserId = o.Id,
                        FullName = o.FullName,
                        Email = o.Email ?? string.Empty,
                        IsActive = o.IsActive
                    })
                    .ToList(),
                TrendLabels = trendLabels,
                TrendValues = trendValues
            };
        }

        private async Task<SocietyPerformanceRowViewModel> BuildSocietyPerformanceRowAsync(
            int societyId,
            string societyName,
            bool isActive,
            IReadOnlyList<Models.Entities.ApplicationUser> operators,
            DateTime overviewDate,
            CancellationToken ct)
        {
            var daily = await _reportService.GetDailyCollectionReportAsync(societyId, overviewDate, ct);
            var farmers = await _farmerRepository.GetBySocietyAsync(societyId, ct);
            var payments = await _paymentRepository.GetBySocietyAsync(societyId, ct);
            var pending = payments.Where(p => p.Status == SettlementStatus.Generated).ToList();
            var collections = await _collectionRepository.GetBySocietyAndDateAsync(societyId, overviewDate, ct);

            return new SocietyPerformanceRowViewModel
            {
                SocietyID = societyId,
                SocietyName = societyName,
                IsActive = isActive,
                OperatorCount = operators.Count(o => o.SocietyID == societyId),
                ActiveFarmerCount = farmers.Count(f => f.IsActive),
                TodayMilkLitres = daily.TotalQuantity,
                TodayPayable = daily.TotalAmount,
                MorningEntries = collections.Count(c => c.Shift == Shift.Morning),
                EveningEntries = collections.Count(c => c.Shift == Shift.Evening),
                PendingSettlementsAmount = pending.Sum(p => p.NetAmount),
                PendingSettlementsCount = pending.Count
            };
        }
    }
}
