using DairyManagementSystem.Models.Entities;
using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Interfaces
{
    public interface IShiftCloseRepository : IRepository<ShiftClose>
    {
        Task<ShiftClose?> GetAsync(int societyId, DateTime date, Shift shift, CancellationToken ct = default);
        Task<List<ShiftClose>> GetBySocietyAndDateAsync(int societyId, DateTime date, CancellationToken ct = default);
    }
}
