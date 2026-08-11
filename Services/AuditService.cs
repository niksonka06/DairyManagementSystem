using System.Text.Json;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public void Log(string entityType, int entityId, AuditAction action, object? oldValue, object? newValue, int performedByUserId)
        {
            var entry = new AuditLog
            {
                EntityType = entityType,
                EntityID = entityId,
                Action = action.ToString(),
                OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
                NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
                PerformedBy = performedByUserId,
                Timestamp = DateTime.UtcNow
            };

            // Staged only — NOT saved here. ApplicationDbContext is Scoped
            // (one instance per HTTP request — see Program.cs), and
            // AuditLogRepository shares that same instance with whatever
            // other repository the calling Service used. So this entry rides
            // along with the next IUnitOfWork.SaveChangesAsync() call the
            // Service makes — same atomic-commit guarantee as before, now
            // via the repository layer instead of a direct DbContext call.
            _auditLogRepository.Add(entry);
        }
    }
}
