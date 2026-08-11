using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Interfaces
{
    public interface IAuditService
    {
        // Deliberately does NOT save changes itself — see AuditService for why.
        // Callers must call SaveChangesAsync on their own DbContext afterward
        // so the business change and its audit entry commit as one unit.
        void Log(string entityType, int entityId, AuditAction action, object? oldValue, object? newValue, int performedByUserId);
    }
}
