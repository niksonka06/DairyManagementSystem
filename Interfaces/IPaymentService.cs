using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IPaymentService
    {
        Task<List<Payment>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<Payment?> GetByIdWithinSocietyAsync(int paymentId, int societyId, CancellationToken ct = default);

        Task<Payment> CreateDraftAsync(SettlementCreateViewModel model, int performedByUserId, CancellationToken ct = default);
        Task RecalculateAsync(int paymentId, int societyId, int performedByUserId, byte[] rowVersion, CancellationToken ct = default);
        Task GenerateAsync(int paymentId, int societyId, int performedByUserId, byte[] rowVersion, CancellationToken ct = default);
        Task MarkPaidAsync(int paymentId, int societyId, int performedByUserId, byte[] rowVersion, CancellationToken ct = default);
        Task CancelDraftAsync(int paymentId, int societyId, int performedByUserId, byte[] rowVersion, CancellationToken ct = default);

        // Admin-only, cross-society — see SettlementUnlockController.
        Task<List<Payment>> GetGeneratedAcrossAllSocietiesAsync(CancellationToken ct = default);
        Task CancelGeneratedAsync(int paymentId, int performedByUserId, string reason, byte[] rowVersion, CancellationToken ct = default);

        // Farmer Portal
        Task<List<Payment>> GetByFarmerAsync(int farmerId, CancellationToken ct = default);
        Task<Payment?> GetByIdForFarmerAsync(int paymentId, int farmerId, CancellationToken ct = default);

        Task<(decimal Amount, DateTime PeriodStart, DateTime PeriodEnd)?> GetCarryForwardAsync(
            int farmerId, DateTime weekReferenceDate, CancellationToken ct = default);
    }
}
