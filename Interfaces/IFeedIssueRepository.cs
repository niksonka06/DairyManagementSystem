using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    public interface IFeedIssueRepository : IRepository<FeedIssue>
    {
        Task<List<FeedIssue>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<List<FeedIssue>> GetByFarmerAsync(int farmerId, int societyId, CancellationToken ct = default);

        Task<List<FeedIssue>> GetUnlockedByFarmerAndPeriodAsync(int farmerId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);

        Task<List<FeedIssue>> GetLockedBySettlementAsync(int paymentId, CancellationToken ct = default);

        Task<List<FeedIssue>> GetBySocietyAndDateRangeAsync(int societyId, DateTime from, DateTime to, CancellationToken ct = default);
    }
}
