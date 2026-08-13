using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IFeedInventoryService
    {
        Task<List<FeedInventory>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<FeedInventory> CreateAsync(FeedInventoryFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task UpdateAsync(FeedInventoryFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task AddStockAsync(AddStockViewModel model, int performedByUserId, CancellationToken ct = default);
        Task SetActiveStatusAsync(int feedItemId, int societyId, bool isActive, int performedByUserId, CancellationToken ct = default);
    }
}
