using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class FarmerRepository : RepositoryBase<Farmer>, IFarmerRepository
    {
        public FarmerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> FarmerCodeExistsInSocietyAsync(string farmerCode, int societyId, int? excludingFarmerId, CancellationToken ct = default)
        {
            var normalized = farmerCode.Trim();
            return await DbSet.AnyAsync(
                f => f.FarmerCode == normalized
                     && f.SocietyID == societyId
                     && f.FarmerID != (excludingFarmerId ?? 0), ct);
        }

        public async Task<List<Farmer>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Where(f => f.SocietyID == societyId)
                .OrderBy(f => f.FullName)
                .ToListAsync(ct);
        }

        public async Task<Farmer?> GetByIdWithinSocietyAsync(int farmerId, int societyId, CancellationToken ct = default)
        {
            return await DbSet
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.FarmerID == farmerId && f.SocietyID == societyId, ct);
        }

        public async Task<Farmer?> GetByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking().FirstOrDefaultAsync(f => f.UserID == userId, ct);
        }
    }
}
