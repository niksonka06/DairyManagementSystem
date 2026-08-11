namespace DairyManagementSystem.Interfaces
{
    // Generic base so every entity-specific repository (ISocietyRepository,
    // IFarmerRepository, etc.) doesn't redeclare the same five methods.
    // Add/Update/Remove are deliberately NOT async and do NOT save — they only
    // stage the change on the tracked DbContext. Nothing touches the database
    // until IUnitOfWork.SaveChangesAsync() is called once by the Service —
    // that's what lets a business change and its audit-log entry commit
    // together as a single atomic operation.
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Remove(TEntity entity);

        // For entities with a RowVersion concurrency token: tells EF Core
        // "compare THIS value (the one the edit form was opened with) against
        // whatever is currently in the database" — not the value on the
        // freshly-reloaded tracked entity, which would always match and
        // silently defeat the whole concurrency check.
        void SetOriginalRowVersion(TEntity entity, byte[] rowVersion);
    }
}
