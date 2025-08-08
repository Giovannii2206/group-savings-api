using GroupSavingsApi.Data;
using Microsoft.EntityFrameworkCore;

using GroupSavingsApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GroupSavingsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddHttpClient();

await MainAsync();

async Task MainAsync()
{
    // Start ngrok
    var ngrokProcess = new System.Diagnostics.Process
    {
        StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "ngrok",
            Arguments = "http 5000 --log=stdout",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };
    ngrokProcess.Start();

    // Wait for ngrok public URL
    string? publicUrl = null;
    using (var http = new HttpClient())
    {
        for (int i = 0; i < 30; i++)
        {
            try
            {
                var resp = await http.GetStringAsync("http://localhost:4040/api/tunnels");
                var doc = System.Text.Json.JsonDocument.Parse(resp);
                var tunnels = doc.RootElement.GetProperty("tunnels");
                foreach (var tunnel in tunnels.EnumerateArray())
                {
                    var proto = tunnel.GetProperty("proto").GetString();
                    if (proto == "https")
                    {
                        publicUrl = tunnel.GetProperty("public_url").GetString();
                        break;
                    }
                }
            }
            catch { }
            if (!string.IsNullOrEmpty(publicUrl)) break;
            await Task.Delay(1000);
        }
    }
    if (string.IsNullOrEmpty(publicUrl))
    {
        Console.WriteLine("Failed to get ngrok public URL");
        ngrokProcess.Kill(true);
        return;
    }

    // Set Telegram webhook
    var botToken = "8344255018:AAFKIxc39IAACmtzeyySWKYwje2wYjL43us";
    var webhookUrl = $"{publicUrl}/api/telegram/webhook";
    var setWebhookUrl = $"https://api.telegram.org/bot{botToken}/setWebhook?url={webhookUrl}";
    using (var http = new HttpClient())
    {
        var resp = await http.GetAsync(setWebhookUrl);
        Console.WriteLine(await resp.Content.ReadAsStringAsync());
    }

    // Start the backend
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowAll");
    // app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    var runTask = app.RunAsync();
    Console.WriteLine($"Backend running at http://localhost:5000, ngrok at {publicUrl}");

    AppDomain.CurrentDomain.ProcessExit += (s, e) =>
    {
        try { ngrokProcess.Kill(true); } catch { }
    };
    Console.CancelKeyPress += (s, e) =>
    {
        try { ngrokProcess.Kill(true); } catch { }
    };

    await runTask;
}
