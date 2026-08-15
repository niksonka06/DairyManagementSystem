using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IFeedIssueService
    {
        Task<List<FeedIssue>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<List<FeedIssue>> GetByFarmerAsync(int farmerId, int societyId, CancellationToken ct = default);
        Task<FeedIssue> IssueToFarmerAsync(FeedIssueFormViewModel model, int performedByUserId, CancellationToken ct = default);
    }
}
