using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    public interface IMilkRateRepository : IRepository<MilkRate>
    {
        Task<List<MilkRate>> GetBySocietyAsync(int societyId, CancellationToken ct = default);

        Task<bool> RangeOverlapsAsync(
            int societyId,
            decimal fatPercentFrom,
            decimal fatPercentTo,
            decimal snfPercentFrom,
            decimal snfPercentTo,
            decimal clrFrom,
            decimal clrTo,
            DateTime effectiveFrom,
            int? excludingRateId,
            CancellationToken ct = default);

        Task<MilkRate?> GetApplicableRateAsync(
            int societyId,
            decimal fatPercent,
            decimal snf,
            decimal clr,
            DateTime collectionDate,
            CancellationToken ct = default);
    }
}
