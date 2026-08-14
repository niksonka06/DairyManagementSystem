namespace DairyManagementSystem.Models.Enums
{
    // Stored as a string in AuditLogs.Action (matches synopsis: NVARCHAR(100)),
    // but kept as a C# enum in code so callers can't typo "Updated" as "Update".
    // Extend this list as later modules need new action types (e.g.
    // SettlementGenerated, SettlementLocked, SettlementUnlocked in Stage 10).
    public enum AuditAction
    {
        Created,
        Updated,
        Deleted,
        Activated,
        Deactivated,
        SettlementGenerated,
        Unlocked
    }
}
