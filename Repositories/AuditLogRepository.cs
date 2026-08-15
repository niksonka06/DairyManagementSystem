using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class AuditLogRepository : RepositoryBase<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
            string? entityType, int? entityId, int? performedBy, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default)
        {
            var query = DbSet.AsNoTracking().Include(a => a.PerformedByUser).AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query = query.Where(a => a.EntityType == entityType);
            }
            if (entityId.HasValue)
            {
                query = query.Where(a => a.EntityID == entityId.Value);
            }
            if (performedBy.HasValue)
            {
                query = query.Where(a => a.PerformedBy == performedBy.Value);
            }
            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp < toDate.Value.Date.AddDays(1));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
