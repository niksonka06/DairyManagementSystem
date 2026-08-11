using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    // No extra methods needed yet beyond the base Add — Stage 14's admin-facing
    // audit viewer will add query methods here (by entity, by user, by date range).
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
    }
}
