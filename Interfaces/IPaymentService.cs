using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IPaymentService
    {
        Task<List<Payment>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<Payment?> GetByIdWithinSocietyAsync(int paymentId, int societyId, CancellationToken ct = default);

        Task<Payment> CreateDraftAsync(SettlementCreateViewModel model, int performedByUserId, CancellationToken ct = default);
        Task RecalculateAsync(int paymentId, int societyId, CancellationToken ct = default);
        Task GenerateAsync(int paymentId, int societyId, int performedByUserId, CancellationToken ct = default);
        Task MarkPaidAsync(int paymentId, int societyId, int performedByUserId, CancellationToken ct = default);
        Task CancelDraftAsync(int paymentId, int societyId, int performedByUserId, CancellationToken ct = default);

        // Admin-only, cross-society — see SettlementUnlockController.
        Task<List<Payment>> GetGeneratedAcrossAllSocietiesAsync(CancellationToken ct = default);
        Task CancelGeneratedAsync(int paymentId, int performedByUserId, string reason, CancellationToken ct = default);
    }
}
