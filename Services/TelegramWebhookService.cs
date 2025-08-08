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
            try
            {
                var http = _httpClientFactory.CreateClient();
                string ngrokApi = "http://localhost:4040/api/tunnels";
                string ngrokResp = await http.GetStringAsync(ngrokApi);
                string publicUrl = ExtractNgrokPublicUrl(ngrokResp);
                if (publicUrl == null)
                {
                    _logger.LogWarning("Could not find public URL from ngrok. Webhook not set.");
                    return;
                }
                string webhookUrl = $"{publicUrl}/api/telegram/webhook";
                string setWebhookUrl = $"https://api.telegram.org/bot{_botToken}/setWebhook?url={webhookUrl}";
                var resp = await http.GetAsync(setWebhookUrl);
                if (resp.IsSuccessStatusCode)
                    _logger.LogInformation("Telegram webhook set to: {WebhookUrl}", webhookUrl);
                else
                    _logger.LogWarning("Failed to set Telegram webhook: {Status}", resp.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning("Could not reach ngrok API or Telegram: {Error}", ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Unexpected error setting Telegram webhook");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private string ExtractNgrokPublicUrl(string ngrokJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(ngrokJson);
                var tunnels = doc.RootElement.GetProperty("tunnels");
                foreach (var tunnel in tunnels.EnumerateArray())
                {
                    if (tunnel.TryGetProperty("public_url", out var urlProp))
                    {
                        var url = urlProp.GetString();
                        if (url != null && url.StartsWith("https://"))
                            return url;
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
