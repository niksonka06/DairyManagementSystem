using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Interfaces
{
    public interface IMilkCollectionRepository : IRepository<MilkCollection>
    {
        Task<List<MilkCollection>> GetBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default);

        Task<MilkCollection?> GetByFarmerDateShiftAsync(int farmerId, DateTime date, Shift shift, int? excludingCollectionId, CancellationToken ct = default);

        Task<MilkCollection?> GetByIdWithinSocietyAsync(int collectionId, int societyId, CancellationToken ct = default);
    }
}
