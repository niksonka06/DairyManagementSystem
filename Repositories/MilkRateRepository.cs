using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class MilkRateRepository : RepositoryBase<MilkRate>, IMilkRateRepository
    {
        public MilkRateRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<MilkRate>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Where(r => r.SocietyID == societyId)
                .OrderByDescending(r => r.EffectiveFrom)
                .ThenBy(r => r.FatPercentFrom)
                .ThenBy(r => r.SnfPercentFrom)
                .ThenBy(r => r.ClrFrom)
                .ToListAsync(ct);
        }

        public async Task<bool> RangeOverlapsAsync(
            int societyId,
            decimal fatPercentFrom,
            decimal fatPercentTo,
            decimal snfPercentFrom,
            decimal snfPercentTo,
            decimal clrFrom,
            decimal clrTo,
            DateTime effectiveFrom,
            int? excludingRateId,
            CancellationToken ct = default)
        {
            return await DbSet.AnyAsync(r =>
                r.SocietyID == societyId &&
                r.EffectiveFrom == effectiveFrom.Date &&
                r.RateID != (excludingRateId ?? 0) &&
                r.FatPercentFrom <= fatPercentTo &&
                fatPercentFrom <= r.FatPercentTo &&
                r.SnfPercentFrom <= snfPercentTo &&
                snfPercentFrom <= r.SnfPercentTo &&
                r.ClrFrom <= clrTo &&
                clrFrom <= r.ClrTo, ct);
        }

        public async Task<MilkRate?> GetApplicableRateAsync(
            int societyId,
            decimal fatPercent,
            decimal snf,
            decimal clr,
            DateTime collectionDate,
            CancellationToken ct = default)
        {
            var date = collectionDate.Date;

            var latestEffectiveFrom = await DbSet
                .Where(r => r.SocietyID == societyId && r.IsActive && r.EffectiveFrom <= date)
                .Select(r => (DateTime?)r.EffectiveFrom)
                .MaxAsync(ct);

            if (latestEffectiveFrom is null)
            {
                return null;
            }

            return await DbSet
                .Where(r => r.SocietyID == societyId
                            && r.IsActive
                            && r.EffectiveFrom == latestEffectiveFrom.Value
                            && r.FatPercentFrom <= fatPercent
                            && fatPercent <= r.FatPercentTo
                            && r.SnfPercentFrom <= snf
                            && snf <= r.SnfPercentTo
                            && r.ClrFrom <= clr
                            && clr <= r.ClrTo)
                .OrderByDescending(r => r.FatPercentFrom)
                .ThenByDescending(r => r.SnfPercentFrom)
                .ThenByDescending(r => r.ClrFrom)
                .FirstOrDefaultAsync(ct);
        }
    }
}
