namespace DairyManagementSystem.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
    }
}
