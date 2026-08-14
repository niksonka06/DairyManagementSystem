using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Interfaces
{
    public interface IMilkCollectionRepository : IRepository<MilkCollection>
    {
        Task<List<MilkCollection>> GetBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default);

        Task<MilkCollection?> GetByFarmerDateShiftAsync(int farmerId, DateTime date, Shift shift, int? excludingCollectionId, CancellationToken ct = default);

        Task<MilkCollection?> GetByIdWithinSocietyAsync(int collectionId, int societyId, CancellationToken ct = default);

        // Used by Settlement generation — only UNLOCKED collections in the
        // period are eligible. Returns tracked entities since Generate needs
        // to set IsLocked=true on each one afterward.
        Task<List<MilkCollection>> GetUnlockedByFarmerAndPeriodAsync(int farmerId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);

        // Used when an Admin cancels a Generated settlement — finds every
        // collection this specific settlement locked, so they can be unlocked.
        Task<List<MilkCollection>> GetLockedBySettlementAsync(int paymentId, CancellationToken ct = default);

        // Society-wide (all farmers, both shifts) total litres for one day —
        // what Dispatch reconciles against. Includes locked collections too,
        // since a collection being part of a farmer's settlement doesn't
        // change how much milk physically left the society that day.
        Task<decimal> GetTotalQuantityBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default);

        // Society-wide, all farmers, for Reports (Daily/Weekly Collection,
        // Fat Analysis, Top Suppliers) — broader than the farmer-scoped
        // queries above, which exist for Settlement generation specifically.
        Task<List<MilkCollection>> GetBySocietyAndDateRangeAsync(int societyId, DateTime from, DateTime to, CancellationToken ct = default);
    }
}
