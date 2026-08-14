using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IAdvancePaymentService
    {
        Task<List<AdvancePayment>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<AdvancePayment> RecordAsync(AdvancePaymentFormViewModel model, int performedByUserId, CancellationToken ct = default);
    }
}
