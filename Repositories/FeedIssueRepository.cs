using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class FeedIssueRepository : RepositoryBase<FeedIssue>, IFeedIssueRepository
    {
        public FeedIssueRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<FeedIssue>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Include(i => i.Farmer)
                .Include(i => i.FeedItem)
                .Where(i => i.SocietyID == societyId)
                .OrderByDescending(i => i.IssueDate)
                .ThenByDescending(i => i.IssueID)
                .ToListAsync(ct);
        }

        public async Task<List<FeedIssue>> GetByFarmerAsync(int farmerId, int societyId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Include(i => i.FeedItem)
                .Where(i => i.FarmerID == farmerId && i.SocietyID == societyId)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync(ct);
        }
    }
}
