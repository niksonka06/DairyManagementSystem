using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DairyManagementSystem.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            _transaction ??= await _context.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction is null)
            {
                return;
            }

            await _context.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction is null)
            {
                return;
            }

            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
