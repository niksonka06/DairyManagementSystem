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
    }
}
