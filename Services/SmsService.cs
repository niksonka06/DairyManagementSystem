using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DairyManagementSystem.Services
{
    public class SmsService : Interfaces.ISmsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<SmsService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(string phoneNumber, string message, CancellationToken ct = default)
        {
            var enabled = _configuration.GetValue<bool>("SmsSettings:Enabled");

            if (!enabled)
            {
                _logger.LogInformation("[SMS SIMULATED - not sent, SmsSettings:Enabled=false] To: {Phone} | Message: {Message}", phoneNumber, message);
                return;
            }

            try
            {
                var apiUrl = _configuration["SmsSettings:ApiUrl"];
                var apiKey = _configuration["SmsSettings:ApiKey"];
                var senderId = _configuration["SmsSettings:SenderId"];

                if (string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("SMS is enabled but ApiUrl/ApiKey are not configured. Message NOT sent to {Phone}.", phoneNumber);
                    return;
                }

                var client = _httpClientFactory.CreateClient("SmsGateway");
                var payload = new Dictionary<string, string>
                {
                    ["to"] = phoneNumber,
                    ["message"] = message,
                    ["sender_id"] = senderId ?? "DAIRYCO",
                    ["api_key"] = apiKey
                };

                using var response = await client.PostAsync(apiUrl, new FormUrlEncodedContent(payload), ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("SMS gateway returned {StatusCode} for {Phone}.", response.StatusCode, phoneNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {Phone}.", phoneNumber);
            }
        }
    }
}