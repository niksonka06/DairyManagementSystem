using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    public interface IMilkRateRepository : IRepository<MilkRate>
    {
        Task<List<MilkRate>> GetBySocietyAsync(int societyId, CancellationToken ct = default);

        Task<bool> RateExistsAsync(int societyId, decimal fatPercent, DateTime effectiveFrom, int? excludingRateId, CancellationToken ct = default);

        // The core business rule from Stage 0: find the rate applicable to a
        // specific society, fat%, and date — using the rate CHART VERSION
        // active on that date (not just any row that happens to match), then
        // nearest-lower-bracket matching within that version. Returns null if
        // no rate chart existed yet for that society/date, or if the fat% is
        // below every bracket in the applicable chart (e.g. chart starts at
        // 3.0% but the collection's fat is 2.7%) — callers must handle both
        // as "no applicable rate", not crash.
        Task<MilkRate?> GetApplicableRateAsync(int societyId, decimal fatPercent, DateTime collectionDate, CancellationToken ct = default);
    }
}
