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
                .ThenByDescending(r => r.FatPercent)
                .ToListAsync(ct);
        }

        public async Task<bool> RateExistsAsync(int societyId, decimal fatPercent, DateTime effectiveFrom, int? excludingRateId, CancellationToken ct = default)
        {
            return await DbSet.AnyAsync(r =>
                r.SocietyID == societyId &&
                r.FatPercent == fatPercent &&
                r.EffectiveFrom == effectiveFrom.Date &&
                r.RateID != (excludingRateId ?? 0), ct);
        }

        public async Task<MilkRate?> GetApplicableRateAsync(int societyId, decimal fatPercent, DateTime collectionDate, CancellationToken ct = default)
        {
            var date = collectionDate.Date;

            // STEP 1: which rate-chart "version" (identified by EffectiveFrom)
            // was active on this date? Only IsActive rows count — a manually
            // deactivated row (e.g. entered by mistake) is never considered,
            // even historically.
            var latestEffectiveFrom = await DbSet
                .Where(r => r.SocietyID == societyId && r.IsActive && r.EffectiveFrom <= date)
                .Select(r => (DateTime?)r.EffectiveFrom)
                .MaxAsync(ct);

            if (latestEffectiveFrom is null)
            {
                return null; // no rate chart existed yet for this society on this date
            }

            // STEP 2: within that specific chart version, nearest-lower-bracket
            // match — the highest FatPercent row that's still <= the actual
            // fat%. If the actual fat is below every bracket in this chart,
            // this returns null and the caller must treat that as "no
            // applicable rate", not silently fall back to some other chart
            // version (that would violate "historical rate must remain correct").
            return await DbSet
                .Where(r => r.SocietyID == societyId
                            && r.IsActive
                            && r.EffectiveFrom == latestEffectiveFrom.Value
                            && r.FatPercent <= fatPercent)
                .OrderByDescending(r => r.FatPercent)
                .FirstOrDefaultAsync(ct);
        }
    }
}
