using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardViewModel> GetUnionDashboardAsync(DateTime? date = null, CancellationToken ct = default);
        Task<SocietyOverviewViewModel?> GetSocietyOverviewAsync(int societyId, DateTime? date = null, CancellationToken ct = default);
    }
}
