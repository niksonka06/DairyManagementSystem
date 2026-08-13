using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Interfaces
{
    public interface IFeedInventoryRepository : IRepository<FeedInventory>
    {
        Task<List<FeedInventory>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<FeedInventory?> GetByIdWithinSocietyAsync(int feedItemId, int societyId, CancellationToken ct = default);
        Task<bool> NameExistsInSocietyAsync(string feedName, ItemType itemType, int societyId, int? excludingFeedItemId, CancellationToken ct = default);
    }
}
