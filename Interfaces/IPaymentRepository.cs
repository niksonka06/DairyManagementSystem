using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<List<Payment>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<Payment?> GetByIdWithinSocietyAsync(int paymentId, int societyId, CancellationToken ct = default);

        // Duplicate-prevention: is there already a non-Cancelled settlement
        // for this farmer covering this exact period?
        Task<bool> ExistsNonCancelledForPeriodAsync(int farmerId, DateTime periodStart, int? excludingPaymentId, CancellationToken ct = default);

        // Powers the "suggested Previous Due" — the most recent non-Cancelled
        // settlement before this period, if its NetAmount was never
        // superseded (we don't track partial payments, so "unpaid" here
        // means Status != Paid).
        Task<Payment?> GetMostRecentUnpaidBeforeAsync(int farmerId, DateTime periodStart, CancellationToken ct = default);

        // Cross-society view for the Admin unlock screen (Stage 10's
        // "unlock requires reason" requirement) — Admin isn't scoped to one
        // society like an Operator is.
        Task<List<Payment>> GetGeneratedAcrossAllSocietiesAsync(CancellationToken ct = default);

        // Farmer Portal — own settlement history and a single settlement's
        // details, both scoped by FarmerID so a farmer can never view (let
        // alone guess the ID of) another farmer's settlement.
        Task<List<Payment>> GetByFarmerAsync(int farmerId, CancellationToken ct = default);
        Task<Payment?> GetByIdForFarmerAsync(int paymentId, int farmerId, CancellationToken ct = default);
    }
}
