using DairyManagementSystem.Models.ViewModels.Reports;

namespace DairyManagementSystem.Interfaces
{
    public interface IReportService
    {
        Task<DailyCollectionReportViewModel> GetDailyCollectionReportAsync(int societyId, DateTime date, CancellationToken ct = default);
        Task<WeeklyCollectionSummaryViewModel> GetWeeklyCollectionSummaryAsync(int societyId, DateTime weekReferenceDate, CancellationToken ct = default);
        Task<FarmerSettlementReportViewModel> GetFarmerSettlementReportAsync(int societyId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
        Task<FeedStockReportViewModel> GetFeedStockReportAsync(int societyId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
        Task<DispatchReportViewModel> GetDispatchReportAsync(int societyId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
        Task<FatAnalysisReportViewModel> GetFatAnalysisReportAsync(int societyId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
        Task<TopSuppliersReportViewModel> GetTopSuppliersReportAsync(int societyId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    }
}
