using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IMilkRateService
    {
        Task<List<MilkRate>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<MilkRate> CreateAsync(MilkRateFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task UpdateAsync(MilkRateFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task SetActiveStatusAsync(int rateId, int societyId, bool isActive, int performedByUserId, CancellationToken ct = default);

        Task<MilkRate?> GetApplicableRateAsync(
            int societyId,
            decimal fatPercent,
            decimal snf,
            decimal clr,
            DateTime collectionDate,
            CancellationToken ct = default);
    }
}
