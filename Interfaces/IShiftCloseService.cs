using DairyManagementSystem.Models.Enums;

namespace DairyManagementSystem.Interfaces
{
    public interface IShiftCloseService
    {
        Task<bool> IsClosedAsync(int societyId, DateTime date, Shift shift, CancellationToken ct = default);
        Task<IReadOnlyCollection<Shift>> GetClosedShiftsAsync(int societyId, DateTime date, CancellationToken ct = default);
        Task CloseAsync(int societyId, DateTime date, Shift shift, int performedByUserId, CancellationToken ct = default);
        Task ReopenAsync(int societyId, DateTime date, Shift shift, int performedByUserId, CancellationToken ct = default);
        Task<bool> CanReopenAsync(int societyId, DateTime date, Shift shift, CancellationToken ct = default);
    }
}
