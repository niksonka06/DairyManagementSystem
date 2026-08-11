using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface ISocietyService
    {
        Task<List<Society>> GetAllAsync(CancellationToken ct = default);
        Task<Society?> GetByIdAsync(int societyId, CancellationToken ct = default);
        Task<Society> CreateAsync(SocietyFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task UpdateAsync(SocietyFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task SetActiveStatusAsync(int societyId, bool isActive, int performedByUserId, CancellationToken ct = default);
    }
}
