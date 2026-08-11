using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    // Shared EF Core plumbing. Entity-specific repositories inherit this for
    // the common CRUD and add their own methods for anything with real query
    // logic (e.g. SocietyRepository.RegistrationNoExistsAsync).
    public class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public RepositoryBase(ApplicationDbContext context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await DbSet.FindAsync(new object[] { id }, ct);
        }

        public async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking().ToListAsync(ct);
        }

        public void Add(TEntity entity) => DbSet.Add(entity);
        public void Update(TEntity entity) => DbSet.Update(entity);
        public void Remove(TEntity entity) => DbSet.Remove(entity);

        public void SetOriginalRowVersion(TEntity entity, byte[] rowVersion)
        {
            // Assumes every entity that calls this has a property literally
            // named "RowVersion" — a convention we keep consistent across
            // every concurrency-tracked entity in this system.
            Context.Entry(entity).Property("RowVersion").OriginalValue = rowVersion;
        }
    }
}
