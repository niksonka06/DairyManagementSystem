using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class FeedInventoryRepository : RepositoryBase<FeedInventory>, IFeedInventoryRepository
    {
        public FeedInventoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<FeedInventory>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Where(f => f.SocietyID == societyId)
                .OrderBy(f => f.ItemType)
                .ThenBy(f => f.FeedName)
                .ToListAsync(ct);
        }

        public async Task<FeedInventory?> GetByIdWithinSocietyAsync(int feedItemId, int societyId, CancellationToken ct = default)
        {
            return await DbSet.FirstOrDefaultAsync(f => f.FeedItemID == feedItemId && f.SocietyID == societyId, ct);
        }

        public async Task<bool> NameExistsInSocietyAsync(string feedName, ItemType itemType, int societyId, int? excludingFeedItemId, CancellationToken ct = default)
        {
            var normalized = feedName.Trim();
            return await DbSet.AnyAsync(f =>
                f.FeedName == normalized &&
                f.ItemType == itemType &&
                f.SocietyID == societyId &&
                f.FeedItemID != (excludingFeedItemId ?? 0), ct);
        }
    }
}
