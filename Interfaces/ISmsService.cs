namespace DairyManagementSystem.Interfaces
{
    public interface ISmsService
    {
        Task SendAsync(string phoneNumber, string message, CancellationToken ct = default);
    }
}