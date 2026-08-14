using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    public interface IAdvancePaymentRepository : IRepository<AdvancePayment>
    {
        Task<List<AdvancePayment>> GetUnappliedByFarmerAsync(int farmerId, CancellationToken ct = default);
        Task<List<AdvancePayment>> GetBySocietyAsync(int societyId, CancellationToken ct = default);

        // Used when cancelling a Generated settlement — finds every advance
        // this settlement consumed, so they can be reverted to unapplied.
        Task<List<AdvancePayment>> GetAppliedToPaymentAsync(int paymentId, CancellationToken ct = default);
    }
}
