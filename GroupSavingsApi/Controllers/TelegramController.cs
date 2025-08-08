using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ILogger<TelegramController> _logger;


    private static readonly Dictionary<long, UserSession> UserStates = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;

    public TelegramController(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider, ILogger<TelegramController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    private const string BotToken = "8344255018:AAFKIxc39IAACmtzeyySWKYwje2wYjL43us";
    private const string ApiBase = "https://api.telegram.org/bot" + BotToken + "/";

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] JsonElement update)
    {
        _logger.LogInformation("Received Telegram webhook POST: {0}", update.ToString());

        if (!update.TryGetProperty("message", out var message))
            return Ok();
        if (!message.TryGetProperty("chat", out var chat))
            return Ok();
        if (!chat.TryGetProperty("id", out var chatIdProp))
            return Ok();
        var chatId = chatIdProp.GetInt64();
        string text = message.GetProperty("text").GetString()?.Trim() ?? "";

        // Track user state
        if (!UserStates.ContainsKey(chatId))
            UserStates[chatId] = new UserSession { Step = SessionStep.MainMenu };
        var state = UserStates[chatId];

        // Handle registration or linking
        if (state.Step == SessionStep.Registering)
        {
            if (text.Contains("@") && text.Contains("."))
            {
                var userId = await RegisterOrLinkUser(chatId, text);
                if (!string.IsNullOrEmpty(userId))
                {
                    state.UserId = userId;
                    state.Step = SessionStep.MainMenu;
                    await SendMessage(chatId, "Registration complete! Returning to menu.");
                }
                else
                {
                    await SendMessage(chatId, "Registration failed. Try again or use a different email.");
                }
            }
            else
            {
                await SendMessage(chatId, "Invalid email address. Please try again.");
            }
        }
        else if (state.Step == SessionStep.MainMenu)
        {
            if (text.ToLower() == "/start")
            {
                await SendMessage(chatId, "Welcome to Group Savings! Please select an option:");
                await SendMessage(chatId, "1. Create a new group");
                await SendMessage(chatId, "2. Join an existing group");
                await SendMessage(chatId, "3. View my groups");
                state.Step = SessionStep.SelectingOption;
            }
            else
            {
                await SendMessage(chatId, "Invalid command. Please try again.");
            }
        }
        else if (state.Step == SessionStep.SelectingOption)
        {
            if (text == "1")
            {
                await SendMessage(chatId, "Please enter a name for your new group:");
                state.Step = SessionStep.CreatingGroup;
            }
            else if (text == "2")
            {
                await SendMessage(chatId, "Please enter the code for the group you want to join:");
                state.Step = SessionStep.JoiningGroup;
            }
            else if (text == "3")
            {
                await SendMessage(chatId, "You are a member of the following groups:");
                // TODO: implement viewing groups
            }
            else
            {
                await SendMessage(chatId, "Invalid option. Please try again.");
            }
        }
        else if (state.Step == SessionStep.CreatingGroup)
        {
            var groupName = text;
            var groupId = await CreateGroup(groupName);
            if (!string.IsNullOrEmpty(groupId))
            {
                await SendMessage(chatId, $"Group '{groupName}' created successfully! Group ID: {groupId}");
                state.Step = SessionStep.MainMenu;
            }
            else
            {
                await SendMessage(chatId, "Failed to create group. Please try again.");
            }
        }
        else if (state.Step == SessionStep.JoiningGroup)
        {
            var groupCode = text;
            var groupId = await JoinGroup(groupCode);
            if (!string.IsNullOrEmpty(groupId))
            {
                await SendMessage(chatId, $"Joined group '{groupId}' successfully!");
                state.Step = SessionStep.MainMenu;
            }
            else
            {
                await SendMessage(chatId, "Failed to join group. Please try again.");
            }
        }

        return Ok();
    }

    [HttpPost("setwebhook")]
    public async Task<IActionResult> SetWebhookManually([FromServices] GroupSavingsApi.Services.TelegramWebhookService webhookService)
    {
        await webhookService.SetWebhook();
        return Ok(new { message = "Webhook set (manual trigger)", url = GroupSavingsApi.Services.TelegramWebhookService.LastWebhookUrl });
    }

    private async Task<string> RegisterOrLinkUser(long chatId, string email)
    {
        // TODO: implement registration or linking logic
        return "";
    }

    private async Task<string> CreateGroup(string groupName)
    {
        // TODO: implement group creation logic
        return "";
    }

    private async Task<string> JoinGroup(string groupCode)
    {
        // TODO: implement group joining logic
        return "";
    }

    private async Task SendMessage(long chatId, string message)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, ApiBase + "sendMessage")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { chat_id = chatId, text = message }), Encoding.UTF8, "application/json")
        };
        await httpClient.SendAsync(request);
    }
}

public enum SessionStep
{
    MainMenu,
    Registering,
    SelectingOption,
    CreatingGroup,
    JoiningGroup
}

public class UserSession
{
    public string UserId { get; set; }
    public SessionStep Step { get; set; }
}
