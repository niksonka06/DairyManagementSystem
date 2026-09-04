using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    // No extra methods needed yet beyond the base Add — Stage 14's admin-facing
    // audit viewer will add query methods here (by entity, by user, by date range).
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        // Filterable, paginated query for the Stage 14 admin viewer. All
        // filter params are optional (null = don't filter on that field).
        Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
            string? entityType, int? entityId, int? performedBy, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default);
    }
}
