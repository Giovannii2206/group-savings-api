using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace GroupSavingsApi.Services
{
    public class NgrokManagerService : IHostedService
    {
        private readonly ILogger<NgrokManagerService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private Process _ngrokProcess;
        private readonly string _ngrokPath = @"C:\\Users\\user\\Downloads\\ngrok-v3-stable-windows-amd64\\ngrok.exe";
        private readonly int _backendPort = 5000;
        private readonly string _botToken = "8344255018:AAFKIxc39IAACmtzeyySWKYwje2wYjL43us";

        public NgrokManagerService(ILogger<NgrokManagerService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // 1. Start ngrok if not already running
            if (!IsNgrokRunning())
            {
                _logger.LogInformation("Starting ngrok on port {Port}...", _backendPort);
                _ngrokProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ngrokPath,
                        Arguments = $"http {_backendPort} --log=stdout",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                _ngrokProcess.Start();
            }
            else
            {
                _logger.LogInformation("ngrok is already running.");
            }

            // 2. Wait for ngrok tunnel to be ready
            string publicUrl = null;
            var http = _httpClientFactory.CreateClient();
            for (int i = 0; i < 20 && publicUrl == null; i++)
            {
                try
                {
                    await Task.Delay(1000, cancellationToken);
                    var ngrokResp = await http.GetStringAsync("http://localhost:4040/api/tunnels");
                    publicUrl = ExtractNgrokPublicUrl(ngrokResp);
                }
                catch { }
            }

            if (publicUrl == null)
            {
                _logger.LogWarning("ngrok tunnel not found after waiting. Telegram webhook not set.");
                return;
            }

            // 3. Set Telegram webhook
            string webhookUrl = $"{publicUrl}/api/telegram/webhook";
            string setWebhookUrl = $"https://api.telegram.org/bot{_botToken}/setWebhook?url={webhookUrl}";
            try
            {
                var resp = await http.GetAsync(setWebhookUrl);
                if (resp.IsSuccessStatusCode)
                    _logger.LogInformation("Telegram webhook set to: {WebhookUrl}", webhookUrl);
                else
                    _logger.LogWarning("Failed to set Telegram webhook: {Status}", resp.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to set Telegram webhook: {Error}", ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_ngrokProcess != null && !_ngrokProcess.HasExited)
                {
                    _ngrokProcess.Kill();
                    _ngrokProcess.Dispose();
                    _logger.LogInformation("ngrok process stopped.");
                }
            }
            catch { }
            return Task.CompletedTask;
        }

        private bool IsNgrokRunning()
        {
            var processes = Process.GetProcessesByName("ngrok");
            return processes.Length > 0;
        }

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
