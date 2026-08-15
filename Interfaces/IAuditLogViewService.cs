using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    // Deliberately separate from IAuditService (Stage 4), which every other
    // module calls to WRITE an audit entry. This one only READS them, for
    // the admin-facing viewer — keeping the two apart means a future change
    // to how logs are queried/displayed can never accidentally affect the
    // write path every other module depends on.
    public interface IAuditLogViewService
    {
        Task<(List<AuditLog> Items, int TotalCount)> SearchAsync(
            string? entityType, int? entityId, int? performedBy, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default);
    }
}
