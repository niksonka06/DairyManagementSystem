using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IFarmerService
    {
        Task<List<Farmer>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<Farmer?> GetByIdWithinSocietyAsync(int farmerId, int societyId, CancellationToken ct = default);
        Task<FarmerCredentialsViewModel> CreateAsync(FarmerFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task UpdateAsync(FarmerFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task SetActiveStatusAsync(int farmerId, int societyId, bool isActive, int performedByUserId, CancellationToken ct = default);
    }
}
