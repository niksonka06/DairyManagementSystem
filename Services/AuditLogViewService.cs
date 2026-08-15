using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Services
{
    public class AuditLogViewService : IAuditLogViewService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogViewService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<(List<AuditLog> Items, int TotalCount)> SearchAsync(
            string? entityType, int? entityId, int? performedBy, DateTime? fromDate, DateTime? toDate,
            int page, int pageSize, CancellationToken ct = default)
        {
            var safePage = page < 1 ? 1 : page;
            var safePageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

            return await _auditLogRepository.GetPagedAsync(entityType, entityId, performedBy, fromDate, toDate, safePage, safePageSize, ct);
        }
    }
}
