using DairyManagementSystem.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DairyManagementSystem.Services
{
    // Generic SMTP via MailKit (Gmail, Outlook, or any relay). When
    // EmailSettings:Enabled is false, the message is logged and not sent so
    // Forgot Password can be tested locally — the reset URL is in the log.
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            var enabled = _configuration.GetValue<bool>("EmailSettings:Enabled");
            var host = _configuration["EmailSettings:SmtpHost"]?.Trim();
            var port = _configuration.GetValue("EmailSettings:SmtpPort", 587);
            var user = _configuration["EmailSettings:SmtpUser"]?.Trim();
            var password = _configuration["EmailSettings:SmtpPassword"]?.Trim();
            var fromName = _configuration["EmailSettings:FromName"]?.Trim() ?? "Smart Dairy Cooperative";
            var fromAddress = _configuration["EmailSettings:FromAddress"]?.Trim();

            _logger.LogWarning(
                "Email send requested. Enabled={Enabled}, Host={Host}, Port={Port}, SmtpUser={SmtpUser}, From={From}, To={To}",
                enabled, host, port, user, fromAddress, toEmail);

            if (!enabled)
            {
                _logger.LogWarning(
                    "[EMAIL SIMULATED - not sent, EmailSettings:Enabled=false] To: {To} | Subject: {Subject} | Body: {Body}",
                    toEmail, subject, htmlBody);
                return false;
            }

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromAddress))
            {
                _logger.LogWarning(
                    "Email is enabled but SmtpHost/SmtpUser/SmtpPassword/FromAddress are not configured. Message NOT sent to {To}.",
                    toEmail);
                return false;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromAddress));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                using var client = new SmtpClient();
                var socketOptions = port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;
                await client.ConnectAsync(host, port, socketOptions, ct);
                await client.AuthenticateAsync(user, password, ct);
                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);

                _logger.LogInformation("Email SENT via SMTP ({Host}:{Port}) to {To}. Subject: {Subject}", host, port, toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}.", toEmail);
                return false;
            }
        }
    }
}
