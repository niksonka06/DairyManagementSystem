using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<ApplicationUser>> GetOperatorsAsync(CancellationToken ct = default);
        Task<OperatorCredentialsViewModel> CreateOperatorAsync(OperatorFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task SetActiveStatusAsync(int userId, bool isActive, int performedByUserId, CancellationToken ct = default);
    }
}
