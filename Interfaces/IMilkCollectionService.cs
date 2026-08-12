using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IMilkCollectionService
    {
        Task<List<MilkCollection>> GetBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default);
        Task<MilkCollection?> GetByIdWithinSocietyAsync(int collectionId, int societyId, CancellationToken ct = default);
        Task<MilkCollection> CreateAsync(MilkCollectionFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task UpdateAsync(MilkCollectionFormViewModel model, int performedByUserId, CancellationToken ct = default);
    }
}
