using DairyManagementSystem.Helpers;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Enums;
using DairyManagementSystem.Models.ViewModels.Reports;

namespace DairyManagementSystem.Services
{
    // No dedicated repository for this service, by design — reports don't
    // own a table of their own, they aggregate across MilkCollection,
    // FeedIssue, Payment, Dispatch, and Farmer. Wrapping "read from 5
    // existing repositories" in a 6th repository would add a layer with no
    // logic of its own — same reasoning that kept Identity's UserManager
    // un-wrapped back in Stage 3/5.
    public class ReportService : IReportService
    {
        private readonly IMilkCollectionRepository _collectionRepository;
        private readonly IFeedIssueRepository _feedIssueRepository;
        private readonly IFeedInventoryRepository _feedInventoryRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IDispatchRepository _dispatchRepository;

        public ReportService(
            IMilkCollectionRepository collectionRepository,
            IFeedIssueRepository feedIssueRepository,
            IFeedInventoryRepository feedInventoryRepository,
            IPaymentRepository paymentRepository,
            IDispatchRepository dispatchRepository)
        {
            _collectionRepository = collectionRepository;
            _feedIssueRepository = feedIssueRepository;
            _feedInventoryRepository = feedInventoryRepository;
            _paymentRepository = paymentRepository;
            _dispatchRepository = dispatchRepository;
        }

        public async Task<DailyCollectionReportViewModel> GetDailyCollectionReportAsync(int societyId, DateTime date, CancellationToken ct = default)
        {
            var day = date.Date;
            var collections = await _collectionRepository.GetBySocietyAndDateRangeAsync(societyId, day, day, ct);

            var morning = collections.Where(c => c.Shift == Shift.Morning).ToList();
            var evening = collections.Where(c => c.Shift == Shift.Evening).ToList();

            return new DailyCollectionReportViewModel
            {
                ReportDate = day,
                TotalQuantity = collections.Sum(c => c.Quantity),
                TotalAmount = collections.Sum(c => c.Amount),
                FarmerCount = collections.Select(c => c.FarmerID).Distinct().Count(),
                AverageFatPercent = collections.Count > 0 ? collections.Average(c => c.FatPercent) : 0,
                MorningQuantity = morning.Sum(c => c.Quantity),
                MorningAmount = morning.Sum(c => c.Amount),
                EveningQuantity = evening.Sum(c => c.Quantity),
                EveningAmount = evening.Sum(c => c.Amount)
            };
        }

        public async Task<WeeklyCollectionSummaryViewModel> GetWeeklyCollectionSummaryAsync(int societyId, DateTime weekReferenceDate, CancellationToken ct = default)
        {
            var (start, end) = DateHelpers.ComputeWeek(weekReferenceDate);
            var collections = await _collectionRepository.GetBySocietyAndDateRangeAsync(societyId, start, end, ct);

            var rows = collections
                .GroupBy(c => new { c.FarmerID, FarmerCode = c.Farmer?.FarmerCode ?? "", FarmerName = c.Farmer?.FullName ?? "" })
                .Select(g => new WeeklyCollectionRow
                {
                    FarmerCode = g.Key.FarmerCode,
                    FarmerName = g.Key.FarmerName,
                    TotalQuantity = g.Sum(c => c.Quantity),
                    AverageFat = g.Average(c => c.FatPercent),
                    GrossAmount = g.Sum(c => c.Amount)
                })
                .OrderBy(r => r.FarmerName)
                .ToList();

            return new WeeklyCollectionSummaryViewModel
            {
                WeekReferenceDate = weekReferenceDate,
                PeriodStart = start,
                PeriodEnd = end,
                Rows = rows
            };
        }

        public async Task<FarmerSettlementReportViewModel> GetFarmerSettlementReportAsync(int societyId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            var payments = await _paymentRepository.GetBySocietyAsync(societyId, ct);

            var rows = payments
                .Where(p => p.PeriodStart >= fromDate.Date && p.PeriodStart <= toDate.Date)
                .Select(p => new SettlementReportRow
                {
                    FarmerCode = p.Farmer?.FarmerCode ?? "",
                    FarmerName = p.Farmer?.FullName ?? "",
                    PeriodStart = p.PeriodStart,
                    PeriodEnd = p.PeriodEnd,
                    GrossAmount = p.GrossAmount,
                    FeedDeduction = p.FeedDeduction,
                    MedicineDeduction = p.MedicineDeduction,
                    OtherDeductionsTotal = p.OtherDeductionsTotal,
                    PreviousDue = p.PreviousDue,
                    AdvancePaid = p.AdvancePaid,
                    NetAmount = p.NetAmount,
                    Status = p.Status
                })
                .OrderByDescending(r => r.PeriodStart)
                .ThenBy(r => r.FarmerName)
                .ToList();

            return new FarmerSettlementReportViewModel
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                Rows = rows,
                TotalNetAmount = rows.Where(r => r.Status != SettlementStatus.Cancelled).Sum(r => r.NetAmount)
            };
        }

        public async Task<FeedStockReportViewModel> GetFeedStockReportAsync(int societyId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            var items = await _feedInventoryRepository.GetBySocietyAsync(societyId, ct);
            var issues = await _feedIssueRepository.GetBySocietyAndDateRangeAsync(societyId, fromDate, toDate, ct);

            var currentStock = items.Select(f => new FeedStockRow
            {
                ItemType = f.ItemType,
                FeedName = f.FeedName,
                Unit = f.Unit,
                StockQuantity = f.StockQuantity,
                PricePerUnit = f.PricePerUnit,
                IsLowStock = f.StockQuantity <= f.LowStockThreshold
            }).OrderBy(r => r.ItemType).ThenBy(r => r.FeedName).ToList();

            var recentIssues = issues.Select(i => new FeedIssueRow
            {
                IssueDate = i.IssueDate,
                FarmerCode = i.Farmer?.FarmerCode ?? "",
                FarmerName = i.Farmer?.FullName ?? "",
                ItemType = i.ItemType,
                FeedName = i.FeedItem?.FeedName ?? "",
                Quantity = i.Quantity,
                TotalCost = i.TotalCost
            }).OrderByDescending(r => r.IssueDate).ToList();

            var farmerTotals = issues
                .GroupBy(i => new { i.FarmerID, FarmerCode = i.Farmer?.FarmerCode ?? "", FarmerName = i.Farmer?.FullName ?? "" })
                .Select(g => new FarmerDeductionTotalRow
                {
                    FarmerCode = g.Key.FarmerCode,
                    FarmerName = g.Key.FarmerName,
                    TotalFeedCost = g.Where(i => i.ItemType == ItemType.Feed).Sum(i => i.TotalCost),
                    TotalMedicineCost = g.Where(i => i.ItemType == ItemType.Medicine).Sum(i => i.TotalCost)
                })
                .OrderBy(r => r.FarmerName)
                .ToList();

            return new FeedStockReportViewModel
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                CurrentStock = currentStock,
                RecentIssues = recentIssues,
                FarmerDeductionTotals = farmerTotals
            };
        }

        public async Task<DispatchReportViewModel> GetDispatchReportAsync(int societyId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            var dispatches = await _dispatchRepository.GetBySocietyAndDateRangeAsync(societyId, fromDate, toDate, ct);

            var rows = dispatches.Select(d => new DispatchReportRow
            {
                DispatchDate = d.DispatchDate,
                VehicleNo = d.VehicleNo,
                Destination = d.Destination,
                TotalCollected = d.TotalCollected,
                TotalDispatched = d.TotalDispatched,
                Variance = d.Variance,
                VariancePercent = d.VariancePercent,
                OperatorName = d.RecordedByUser?.FullName ?? ""
            }).ToList();

            return new DispatchReportViewModel
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                Rows = rows
            };
        }

        public async Task<FatAnalysisReportViewModel> GetFatAnalysisReportAsync(int societyId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            var collections = await _collectionRepository.GetBySocietyAndDateRangeAsync(societyId, fromDate, toDate, ct);

            var farmerAverages = collections
                .GroupBy(c => new { c.FarmerID, FarmerCode = c.Farmer?.FarmerCode ?? "", FarmerName = c.Farmer?.FullName ?? "" })
                .Select(g => new FarmerFatRow
                {
                    FarmerCode = g.Key.FarmerCode,
                    FarmerName = g.Key.FarmerName,
                    AverageFat = g.Average(c => c.FatPercent),
                    CollectionCount = g.Count()
                })
                .OrderByDescending(r => r.AverageFat)
                .ToList();

            // Fixed buckets across the valid 2.5-9.0 range, one point per
            // 1.0% band — matches the synopsis's "fat distribution histogram".
            var bucketBounds = new (decimal Low, decimal High)[]
            {
                (2.5m, 3.5m), (3.5m, 4.5m), (4.5m, 5.5m), (5.5m, 6.5m), (6.5m, 7.5m), (7.5m, 9.0m)
            };
            var distribution = bucketBounds.Select(b => new FatBucket
            {
                Label = $"{b.Low:0.0} - {b.High:0.0}%",
                Count = collections.Count(c => c.FatPercent >= b.Low && c.FatPercent < b.High)
                        + (b.High == 9.0m ? collections.Count(c => c.FatPercent == 9.0m) : 0) // include the exact upper bound in the last bucket
            }).ToList();

            var trend = collections
                .GroupBy(c => c.CollectionDate)
                .Select(g => new FatTrendPoint { Date = g.Key, AverageFat = g.Average(c => c.FatPercent) })
                .OrderBy(t => t.Date)
                .ToList();

            return new FatAnalysisReportViewModel
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                FarmerAverages = farmerAverages,
                Distribution = distribution,
                Trend = trend
            };
        }

        public async Task<TopSuppliersReportViewModel> GetTopSuppliersReportAsync(int societyId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
        {
            var collections = await _collectionRepository.GetBySocietyAndDateRangeAsync(societyId, fromDate, toDate, ct);

            var byFarmer = collections
                .GroupBy(c => new { c.FarmerID, FarmerCode = c.Farmer?.FarmerCode ?? "", FarmerName = c.Farmer?.FullName ?? "" })
                .Select(g => new
                {
                    g.Key.FarmerCode,
                    g.Key.FarmerName,
                    TotalQuantity = g.Sum(c => c.Quantity),
                    TotalAmount = g.Sum(c => c.Amount)
                })
                .ToList();

            return new TopSuppliersReportViewModel
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                TopByQuantity = byFarmer.OrderByDescending(f => f.TotalQuantity).Take(10)
                    .Select(f => new SupplierRow { FarmerCode = f.FarmerCode, FarmerName = f.FarmerName, Value = f.TotalQuantity }).ToList(),
                TopByIncome = byFarmer.OrderByDescending(f => f.TotalAmount).Take(10)
                    .Select(f => new SupplierRow { FarmerCode = f.FarmerCode, FarmerName = f.FarmerName, Value = f.TotalAmount }).ToList()
            };
        }
    }
}
