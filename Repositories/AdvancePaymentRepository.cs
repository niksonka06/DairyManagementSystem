using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class AdvancePaymentRepository : RepositoryBase<AdvancePayment>, IAdvancePaymentRepository
    {
        public AdvancePaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<AdvancePayment>> GetUnappliedByFarmerAsync(int farmerId, DateTime? paidOnOrBefore = null, CancellationToken ct = default)
        {
            var query = DbSet.Where(a => a.FarmerID == farmerId && !a.IsApplied);
            if (paidOnOrBefore.HasValue)
            {
                var cutoff = paidOnOrBefore.Value.Date;
                query = query.Where(a => a.PaymentDate <= cutoff);
            }

            return await query.ToListAsync(ct);
        }

        public async Task<List<AdvancePayment>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Include(a => a.Farmer)
                .Where(a => a.SocietyID == societyId)
                .OrderByDescending(a => a.PaymentDate)
                .ToListAsync(ct);
        }

        public async Task<List<AdvancePayment>> GetAppliedToPaymentAsync(int paymentId, CancellationToken ct = default)
        {
            return await DbSet.Where(a => a.AppliedToPaymentID == paymentId).ToListAsync(ct);
        }
    }
}
