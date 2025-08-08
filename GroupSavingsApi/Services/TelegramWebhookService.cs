using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GroupSavingsApi.Services
{
    public class TelegramWebhookService : IHostedService
    {
        private static string? _lastWebhookUrl;
        public static string? LastWebhookUrl => _lastWebhookUrl;

        private readonly ILogger<TelegramWebhookService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly string _botToken;

        public TelegramWebhookService(ILogger<TelegramWebhookService> logger, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _botToken = "8344255018:AAFKIxc39IAACmtzeyySWKYwje2wYjL43us"; // Provided by user
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Always attempt to set the webhook on startup, regardless of previous state
            await SetWebhook(cancellationToken);
        }

        public async Task SetWebhook(CancellationToken cancellationToken = default)
        {
            try
            {
                // Wait for ngrok public URL
                string? publicUrl = null;
                for (int i = 0; i < 30; i++) // longer wait
                {
                    publicUrl = GroupSavingsApi.Services.NgrokManagerService.PublicUrl;
                    if (!string.IsNullOrEmpty(publicUrl))
                        break;
                    _logger.LogInformation($"Waiting for ngrok public URL... attempt {i+1}");
                    await Task.Delay(1000, cancellationToken);
                }
                if (string.IsNullOrEmpty(publicUrl))
                {
                    _logger.LogWarning("Ngrok public URL not found. Webhook not set.");
                    return;
                }
                string webhookUrl = $"{publicUrl}/api/telegram/webhook";
                string setWebhookUrl = $"https://api.telegram.org/bot{_botToken}/setWebhook?url={webhookUrl}";
                _lastWebhookUrl = webhookUrl;
                _logger.LogInformation($"Setting Telegram webhook to: {webhookUrl}");
                var http = _httpClientFactory.CreateClient();
                var resp = await http.GetAsync(setWebhookUrl, cancellationToken);
                if (resp.IsSuccessStatusCode)
                    _logger.LogInformation($"Telegram webhook set to: {webhookUrl}");
                else
                    _logger.LogWarning($"Failed to set Telegram webhook: {resp.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Telegram webhook");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
