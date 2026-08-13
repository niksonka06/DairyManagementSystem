using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    public interface IFeedIssueRepository : IRepository<FeedIssue>
    {
        Task<List<FeedIssue>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<List<FeedIssue>> GetByFarmerAsync(int farmerId, int societyId, CancellationToken ct = default);
    }
}
