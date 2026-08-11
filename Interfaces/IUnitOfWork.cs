namespace DairyManagementSystem.Interfaces
{
    // One SaveChangesAsync() call, shared by whatever repositories a Service
    // used during this request. This is the "commit" step — repositories
    // only stage changes, this is what actually writes them to SQL Server,
    // as one transaction covering everything staged so far.
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
