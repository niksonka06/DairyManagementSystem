using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class DispatchRepository : RepositoryBase<Dispatch>, IDispatchRepository
    {
        public DispatchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Dispatch>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Where(d => d.SocietyID == societyId)
                .OrderByDescending(d => d.DispatchDate)
                .ToListAsync(ct);
        }

        public async Task<Dispatch?> GetByIdWithinSocietyAsync(int dispatchId, int societyId, CancellationToken ct = default)
        {
            return await DbSet.FirstOrDefaultAsync(d => d.DispatchID == dispatchId && d.SocietyID == societyId, ct);
        }

        public async Task<bool> ExistsForDateAsync(int societyId, DateTime date, int? excludingDispatchId, CancellationToken ct = default)
        {
            var day = date.Date;
            return await DbSet.AnyAsync(d =>
                d.SocietyID == societyId &&
                d.DispatchDate == day &&
                d.DispatchID != (excludingDispatchId ?? 0), ct);
        }

        public async Task<List<Dispatch>> GetBySocietyAndDateRangeAsync(int societyId, DateTime from, DateTime to, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Include(d => d.RecordedByUser)
                .Where(d => d.SocietyID == societyId && d.DispatchDate >= from.Date && d.DispatchDate <= to.Date)
                .OrderByDescending(d => d.DispatchDate)
                .ToListAsync(ct);
        }
    }
}
