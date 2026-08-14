using DairyManagementSystem.Models.Entities;

namespace DairyManagementSystem.Interfaces
{
    public interface IDispatchRepository : IRepository<Dispatch>
    {
        Task<List<Dispatch>> GetBySocietyAsync(int societyId, CancellationToken ct = default);
        Task<Dispatch?> GetByIdWithinSocietyAsync(int dispatchId, int societyId, CancellationToken ct = default);
        Task<bool> ExistsForDateAsync(int societyId, DateTime date, int? excludingDispatchId, CancellationToken ct = default);
        Task<List<Dispatch>> GetBySocietyAndDateRangeAsync(int societyId, DateTime from, DateTime to, CancellationToken ct = default);
    }
}
