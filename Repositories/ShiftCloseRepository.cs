using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class ShiftCloseRepository : RepositoryBase<ShiftClose>, IShiftCloseRepository
    {
        public ShiftCloseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ShiftClose?> GetAsync(int societyId, DateTime date, Shift shift, CancellationToken ct = default)
        {
            var day = date.Date;
            return await DbSet.FirstOrDefaultAsync(
                s => s.SocietyID == societyId && s.CollectionDate == day && s.Shift == shift, ct);
        }

        public async Task<List<ShiftClose>> GetBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default)
        {
            var day = date.Date;
            return await DbSet.AsNoTracking()
                .Where(s => s.SocietyID == societyId && s.CollectionDate == day)
                .ToListAsync(ct);
        }
    }
}
