namespace DairyManagementSystem.Interfaces
{
    // One SaveChangesAsync() call, shared by whatever repositories a Service
    // used during this request. This is the "commit" step — repositories
    // only stage changes, this is what actually writes them to SQL Server,
    // as one transaction covering everything staged so far.
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        // For multi-step operations that mix repository writes with calls
        // that persist independently (e.g. Identity's UserManager.CreateAsync,
        // which commits itself as soon as it's called). Since UserManager and
        // our repositories share the same scoped DbContext, wrapping both in
        // an explicit transaction here means either everything commits
        // together, or a failure partway through rolls EVERYTHING back —
        // no orphaned login account with no matching Farmer row, or vice versa.
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync(CancellationToken ct = default);
    }
}
