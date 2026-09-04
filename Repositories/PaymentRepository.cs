using DairyManagementSystem.Data;
using DairyManagementSystem.Interfaces;
using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DairyManagementSystem.Repositories
{
    public class PaymentRepository : RepositoryBase<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Payment>> GetBySocietyAsync(int societyId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Include(p => p.Farmer)
                .Where(p => p.SocietyID == societyId)
                .OrderByDescending(p => p.PeriodStart)
                .ThenBy(p => p.Farmer!.FullName)
                .ToListAsync(ct);
        }

        public async Task<Payment?> GetByIdWithinSocietyAsync(int paymentId, int societyId, CancellationToken ct = default)
        {
            return await DbSet
                .Include(p => p.Farmer)
                .Include(p => p.Deductions)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId && p.SocietyID == societyId, ct);
        }

        public async Task<bool> ExistsNonCancelledForPeriodAsync(int farmerId, DateTime periodStart, int? excludingPaymentId, CancellationToken ct = default)
        {
            return await DbSet.AnyAsync(p =>
                p.FarmerID == farmerId &&
                p.PeriodStart == periodStart.Date &&
                p.Status != SettlementStatus.Cancelled &&
                p.PaymentID != (excludingPaymentId ?? 0), ct);
        }

        public async Task<Payment?> GetMostRecentBeforeAsync(int farmerId, DateTime periodStart, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Where(p => p.FarmerID == farmerId
                            && p.PeriodStart < periodStart.Date
                            && (p.Status == SettlementStatus.Generated || p.Status == SettlementStatus.Paid))
                .OrderByDescending(p => p.PeriodStart)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<Payment>> GetGeneratedAcrossAllSocietiesAsync(CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Include(p => p.Farmer)
                .Include(p => p.Society)
                .Where(p => p.Status == SettlementStatus.Generated)
                .OrderByDescending(p => p.GeneratedAt)
                .ToListAsync(ct);
        }

        public async Task<List<Payment>> GetByFarmerAsync(int farmerId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Where(p => p.FarmerID == farmerId)
                .OrderByDescending(p => p.PeriodStart)
                .ToListAsync(ct);
        }

        public async Task<Payment?> GetByIdForFarmerAsync(int paymentId, int farmerId, CancellationToken ct = default)
        {
            return await DbSet.AsNoTracking()
                .Include(p => p.Deductions)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId && p.FarmerID == farmerId, ct);
        }
    }
}
