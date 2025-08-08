using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GroupSavingsApi.Services
{
    public class NgrokManagerService : IHostedService
    {
        private readonly ILogger<NgrokManagerService> _logger;
        private Process? _ngrokProcess;
        public static string? PublicUrl { get; private set; }

        public NgrokManagerService(ILogger<NgrokManagerService> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Wait for backend to be live
                using var healthCheckClient = new HttpClient();
                var healthCheckUrl = "http://localhost:5000/health";
                _logger.LogInformation("Waiting for backend to be live at {0}", healthCheckUrl);
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        var resp = await healthCheckClient.GetAsync(healthCheckUrl, cancellationToken);
                        if (resp.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Backend is live.");
                            break;
                        }
                    }
                    catch { }
                    await Task.Delay(1000, cancellationToken);
                }
                // Start ngrok on port 80
                _ngrokProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ngrok",
                        Arguments = "http 5000 --log=stdout",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                _ngrokProcess.Start();

                // Wait for ngrok to initialize and get public URL
                using var http = new HttpClient();
                for (int i = 0; i < 15; i++)
                {
                    try
                    {
                        var resp = await http.GetStringAsync("http://localhost:4040/api/tunnels");
                        var doc = JsonDocument.Parse(resp);
                        var tunnels = doc.RootElement.GetProperty("tunnels");
                        foreach (var tunnel in tunnels.EnumerateArray())
                        {
                            var proto = tunnel.GetProperty("proto").GetString();
                            if (proto == "https")
                            {
                                PublicUrl = tunnel.GetProperty("public_url").GetString();
                                _logger.LogInformation($"ngrok public URL: {PublicUrl}");
                                return;
                            }
                        }
                    }
                    catch { }
                    await Task.Delay(1000);
                }
                _logger.LogWarning("ngrok public URL not found after waiting.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to start ngrok");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_ngrokProcess != null && !_ngrokProcess.HasExited)
                    _ngrokProcess.Kill();
            }
            catch { }
            return Task.CompletedTask;
        }
    }
}
