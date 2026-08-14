using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class MilkCollectionRepository : RepositoryBase<MilkCollection>, IMilkCollectionRepository
    {
        public MilkCollectionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<MilkCollection>> GetBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default)
        {
            var day = date.Date;
            return await DbSet.AsNoTracking()
                .Include(c => c.Farmer)
                .Where(c => c.SocietyID == societyId && c.CollectionDate == day)
                .OrderBy(c => c.Shift)
                .ThenBy(c => c.Farmer!.FullName)
                .ToListAsync(ct);
        }

        public async Task<MilkCollection?> GetByFarmerDateShiftAsync(int farmerId, DateTime date, Shift shift, int? excludingCollectionId, CancellationToken ct = default)
        {
            var day = date.Date;
            return await DbSet.FirstOrDefaultAsync(c =>
                c.FarmerID == farmerId &&
                c.CollectionDate == day &&
                c.Shift == shift &&
                c.CollectionID != (excludingCollectionId ?? 0), ct);
        }

        public async Task<MilkCollection?> GetByIdWithinSocietyAsync(int collectionId, int societyId, CancellationToken ct = default)
        {
            return await DbSet
                .Include(c => c.Farmer)
                .FirstOrDefaultAsync(c => c.CollectionID == collectionId && c.SocietyID == societyId, ct);
        }

        public async Task<List<MilkCollection>> GetUnlockedByFarmerAndPeriodAsync(int farmerId, DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
        {
            return await DbSet
                .Where(c => c.FarmerID == farmerId
                            && !c.IsLocked
                            && c.CollectionDate >= periodStart.Date
                            && c.CollectionDate <= periodEnd.Date)
                .ToListAsync(ct);
        }

        public async Task<List<MilkCollection>> GetLockedBySettlementAsync(int paymentId, CancellationToken ct = default)
        {
            return await DbSet.Where(c => c.LockedBySettlementID == paymentId).ToListAsync(ct);
        }

        public async Task<decimal> GetTotalQuantityBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default)
        {
            var day = date.Date;
            return await DbSet
                .Where(c => c.SocietyID == societyId && c.CollectionDate == day)
                .SumAsync(c => (decimal?)c.Quantity, ct) ?? 0m;
        }

        public async Task<List<MilkCollection>> GetBySocietyAndDateRangeAsync(int societyId, DateTime from, DateTime to, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Include(c => c.Farmer)
                .Where(c => c.SocietyID == societyId && c.CollectionDate >= from.Date && c.CollectionDate <= to.Date)
                .ToListAsync(ct);
        }
    }
}
