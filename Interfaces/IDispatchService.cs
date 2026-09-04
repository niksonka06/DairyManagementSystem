using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.ViewModels;

namespace DairyManagementSystem.Interfaces
{
    public interface IDispatchService
    {
        Task<List<Dispatch>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<Dispatch?> GetByIdWithinSocietyAsync(int dispatchId, int societyId, CancellationToken ct = default);
        Task<Dispatch> CreateAsync(DispatchFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task UpdateAsync(DispatchFormViewModel model, int performedByUserId, CancellationToken ct = default);
        Task<decimal> GetCollectedLitresAsync(int societyId, DateTime date, CancellationToken ct = default);
    }
}
