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

    private static readonly Dictionary<long, UserSession> UserStates = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceProvider _serviceProvider;

    public TelegramController(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _serviceProvider = serviceProvider;
    }
    private const string BotToken = "8344255018:AAFKIxc39IAACmtzeyySWKYwje2wYjL43us";
    private const string ApiBase = "https://api.telegram.org/bot" + BotToken + "/";

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] JsonElement update)
    {
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
                await SendMessage(chatId, "Please enter a valid email address:");
            }
            return Ok();
        }

        // If not registered, prompt for registration
        if (string.IsNullOrEmpty(state.UserId))
        {
            state.Step = SessionStep.Registering;
            await SendMessage(chatId, "Welcome! Please enter your email to register or link your account:");
            return Ok();
        }

        // Main menu logic
        if (state.Step == SessionStep.MainMenu)
        {
            if (text == "/start" || string.IsNullOrEmpty(text))
            {
                await SendMessage(chatId, GetMenu());
                return Ok();
            }
            if (text == "1") // Check Balance
            {
                var balance = await GetBalance(state.UserId);
                await SendMessage(chatId, $"Your balance: {balance}");
                await SendMessage(chatId, GetMenu());
                return Ok();
            }
            if (text == "2") // Make Contribution
            {
                state.Step = SessionStep.Contribute_Amount;
                await SendMessage(chatId, "Enter contribution amount:");
                return Ok();
            }
            if (text == "3") // Join Group
            {
                state.Step = SessionStep.JoinGroup;
                await SendMessage(chatId, "Enter group code or name to join:");
                return Ok();
            }
            if (text == "4") // View My Groups
            {
                state.Step = SessionStep.ViewGroups;
                await SendMessage(chatId, await ListUserGroups(state.UserId));
                await SendMessage(chatId, "Reply with group number to select, or /menu to return.");
                return Ok();
            }
            if (text == "5") // View Group Sessions
            {
                state.Step = SessionStep.ViewSessions_SelectGroup;
                await SendMessage(chatId, await ListUserGroups(state.UserId));
                await SendMessage(chatId, "Reply with group number to view sessions, or /menu to return.");
                return Ok();
            }
            if (text == "6") // View Contributions
            {
                state.Step = SessionStep.ViewContributions;
                await SendMessage(chatId, await ListUserContributions(state.UserId));
                await SendMessage(chatId, GetMenu());
                return Ok();
            }
            if (text == "7") // View Savings Goals
            {
                state.Step = SessionStep.ViewGoals_SelectGroup;
                await SendMessage(chatId, await ListUserGroups(state.UserId));
                await SendMessage(chatId, "Reply with group number to view savings goals, or /menu to return.");
                return Ok();
            }
            if (text == "8") // Manage Payment Methods
            {
                state.Step = SessionStep.ManagePaymentMethods;
                await SendMessage(chatId, await ListPaymentMethods(state.UserId));
                await SendMessage(chatId, "Reply ADD to add, DEL <#> to delete, or /menu to return.");
                return Ok();
            }
            if (text == "9") // View Notifications
            {
                state.Step = SessionStep.ViewNotifications;
                await SendMessage(chatId, await ListNotifications(state.UserId));
                await SendMessage(chatId, GetMenu());
                return Ok();
            }
            if (text == "10") // View Audit Log
            {
                state.Step = SessionStep.ViewAuditLog;
                await SendMessage(chatId, await ListAuditLog(state.UserId));
                await SendMessage(chatId, GetMenu());
                return Ok();
            }
            if (text == "11") // Reports
            {
                state.Step = SessionStep.ViewReports;
                await SendMessage(chatId, await ListReports(state.UserId));
                await SendMessage(chatId, GetMenu());
                return Ok();
            }
            if (text == "12") // Help / About
            {
                state.Step = SessionStep.HelpAbout;
                await SendMessage(chatId, GetHelpText());
                await SendMessage(chatId, GetMenu());
                return Ok();
            }
            await SendMessage(chatId, "Invalid option. Reply with 1, 2, or 3.");
            await SendMessage(chatId, GetMenu());
            return Ok();
        }
        // Contribution flow
        if (state.Step == SessionStep.Contribute_Amount)
        {
            if (decimal.TryParse(text, out var amount))
            {
                state.TempAmount = amount;
                state.Step = SessionStep.Contribute_Confirm;
                await SendMessage(chatId, $"Confirm contribution of {amount}? Reply YES to confirm, NO to cancel.");
            }
            else
            {
                await SendMessage(chatId, "Invalid amount. Enter a number:");
            }
            return Ok();
        }
        if (state.Step == SessionStep.Contribute_Confirm)
        {
            if (text.ToLower() == "yes")
            {
                var result = await MakeContribution(state.UserId, state.TempAmount);
                await SendMessage(chatId, result ? "Contribution successful!" : "Contribution failed.");
                state.Step = SessionStep.MainMenu;
                await SendMessage(chatId, GetMenu());
            }
            else
            {
                await SendMessage(chatId, "Contribution cancelled.");
                state.Step = SessionStep.MainMenu;
                await SendMessage(chatId, GetMenu());
            }
            return Ok();
        }
        // Join group flow
        if (state.Step == SessionStep.JoinGroup)
        {
            var joined = await JoinGroup(state.UserId, text);
            await SendMessage(chatId, joined ? "Joined group!" : "Failed to join group.");
            state.Step = SessionStep.MainMenu;
            await SendMessage(chatId, GetMenu());
            return Ok();
        }
        // Fallback
        await SendMessage(chatId, "Unexpected input. Returning to menu.");
        state.Step = SessionStep.MainMenu;
        await SendMessage(chatId, GetMenu());
        return Ok();
    }

    private string GetMenu()
    {
        return "Main Menu:\n" +
            "1. Check Balance\n" +
            "2. Make Contribution\n" +
            "3. Join Group\n" +
            "4. View My Groups\n" +
            "5. View Group Sessions\n" +
            "6. View Contributions\n" +
            "7. View Savings Goals\n" +
            "8. Manage Payment Methods\n" +
            "9. View Notifications\n" +
            "10. View Audit Log\n" +
            "11. Reports\n" +
            "12. Help / About\n" +
            "Reply with the number of your choice.";
    }

    // --- API Integration Helpers ---
    private async Task<string> RegisterOrLinkUser(long chatId, string email)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = (GroupSavingsApi.Data.GroupSavingsDbContext)scope.ServiceProvider.GetService(typeof(GroupSavingsApi.Data.GroupSavingsDbContext));
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
            return user.Id.ToString();
        // Register new user
        var newUser = new GroupSavingsApi.Models.User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("changeme"),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(newUser);
        await db.SaveChangesAsync();
        return newUser.Id.ToString();
    }

    private async Task<string> GetBalance(string userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = (GroupSavingsApi.Data.GroupSavingsDbContext)scope.ServiceProvider.GetService(typeof(GroupSavingsApi.Data.GroupSavingsDbContext));
        var groupMember = await db.GroupMembers.FirstOrDefaultAsync(m => m.UserId.ToString() == userId);
        if (groupMember == null) return "No balance found. Join a group first.";
        return groupMember.TotalContributed.ToString("C");
    }

    private async Task<bool> MakeContribution(string userId, decimal amount)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = (GroupSavingsApi.Data.GroupSavingsDbContext)scope.ServiceProvider.GetService(typeof(GroupSavingsApi.Data.GroupSavingsDbContext));
        var member = await db.Members.FirstOrDefaultAsync(m => m.UserId.ToString() == userId);
        if (member == null) return false;
        member.AccountBalance += amount;
        await db.SaveChangesAsync();
        return true;
    }

    private async Task<bool> JoinGroup(string userId, string groupCodeOrName)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = (GroupSavingsApi.Data.GroupSavingsDbContext)scope.ServiceProvider.GetService(typeof(GroupSavingsApi.Data.GroupSavingsDbContext));
        var group = await db.Groups.FirstOrDefaultAsync(g => g.Name == groupCodeOrName || g.Id.ToString() == groupCodeOrName);
        if (group == null) return false;
        var groupMember = await db.GroupMembers.FirstOrDefaultAsync(m => m.UserId.ToString() == userId && m.GroupId == group.Id);
        if (groupMember != null) return true; // already a member
        db.GroupMembers.Add(new GroupSavingsApi.Models.GroupMember {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = Guid.Parse(userId),
            RoleId = 1, // Default role
            JoinedAt = DateTime.UtcNow,
            TotalContributed = 0,
            IsDeleted = false
        });
        await db.SaveChangesAsync();
        return true;
    }

    private class UserSession
    {
        public string UserId { get; set; }
        public SessionStep Step { get; set; } = SessionStep.MainMenu;
        public decimal TempAmount { get; set; }
        public int? TempGroupIndex { get; set; }
        public int? TempSessionIndex { get; set; }
        public int? TempPaymentIndex { get; set; }
    }
    private enum SessionStep
    {
        MainMenu,
        Registering,
        Contribute_Amount,
        Contribute_Confirm,
        JoinGroup,
        ViewGroups,
        ViewSessions_SelectGroup,
        ViewSessions,
        ViewContributions,
        ViewGoals_SelectGroup,
        ViewGoals,
        ManagePaymentMethods,
        AddPaymentMethod,
        DeletePaymentMethod,
        ViewNotifications,
        ViewAuditLog,
        ViewReports,
        HelpAbout
    }

    // --- Menu Helper Stubs ---
    private async Task<string> ListUserGroups(string userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = (GroupSavingsApi.Data.GroupSavingsDbContext)scope.ServiceProvider.GetService(typeof(GroupSavingsApi.Data.GroupSavingsDbContext));
        var userGuid = Guid.Parse(userId);
        var groupMembers = db.GroupMembers.Where(m => m.UserId == userGuid && !m.IsDeleted).ToList();
        if (!groupMembers.Any()) return "You are not a member of any group.";
        var groupIds = groupMembers.Select(m => m.GroupId).ToList();
        var groups = db.Groups.Where(g => groupIds.Contains(g.Id) && !g.IsDeleted).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("Your Groups:");
        int idx = 1;
        foreach (var g in groups)
        {
            sb.AppendLine($"{idx}. {g.Name} (ID: {g.Id})");
            idx++;
        }
        return sb.ToString();
    }
    private async Task<string> ListUserContributions(string userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = (GroupSavingsApi.Data.GroupSavingsDbContext)scope.ServiceProvider.GetService(typeof(GroupSavingsApi.Data.GroupSavingsDbContext));
        var userGuid = Guid.Parse(userId);
        var contributions = db.Contributions.Where(c => c.UserId == userGuid && !c.IsDeleted).OrderByDescending(c => c.Date).ToList();
        if (!contributions.Any()) return "You have not made any contributions.";
        var sb = new StringBuilder();
        sb.AppendLine("Your Contributions:");
        int idx = 1;
        foreach (var c in contributions)
        {
            var session = db.GroupSessions.FirstOrDefault(s => s.Id == c.GroupSessionId);
            var group = session != null ? db.Groups.FirstOrDefault(g => g.Id == session.GroupId) : null;
            sb.AppendLine($"{idx}. {c.Amount:C} (Session: {session?.Id.ToString().Substring(0, 8) ?? "-"}) on {c.Date:d}");
            idx++;
        }
        return sb.ToString();
    }
    private async Task<string> ListUserSessions(string userId, int groupIndex)
    {
        // TODO: Fetch sessions for group
        return "[Sessions for group will be listed here]";
    }
    private async Task<string> ListUserGoals(string userId, int groupIndex)
    {
        // TODO: Fetch savings goals for group
        return "[Savings goals will be listed here]";
    }
    private async Task<string> ListPaymentMethods(string userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = (GroupSavingsApi.Data.GroupSavingsDbContext)scope.ServiceProvider.GetService(typeof(GroupSavingsApi.Data.GroupSavingsDbContext));
        var userGuid = Guid.Parse(userId);
        var methods = db.PaymentMethods.Where(p => p.CustomerId == userGuid).ToList();
        if (!methods.Any()) return "You have no payment methods.";
        var sb = new StringBuilder();
        sb.AppendLine("Your Payment Methods:");
        int idx = 1;
        foreach (var m in methods)
        {
            sb.AppendLine($"{idx}. {m.ProviderName} - {m.AccountNumber} ({m.Currency})");
            idx++;
        }
        return sb.ToString();
    }
    private async Task<string> ListNotifications(string userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = (GroupSavingsApi.Data.GroupSavingsDbContext)scope.ServiceProvider.GetService(typeof(GroupSavingsApi.Data.GroupSavingsDbContext));
        var userGuid = Guid.Parse(userId);
        var notifications = db.Notifications.Where(n => n.UserId == userGuid).OrderByDescending(n => n.CreatedAt).ToList();
        if (!notifications.Any()) return "You have no notifications.";
        var sb = new StringBuilder();
        sb.AppendLine("Your Notifications:");
        int idx = 1;
        foreach (var n in notifications)
        {
            sb.AppendLine($"{idx}. [{n.Type}] {n.Message} ({n.CreatedAt:d})");
            idx++;
        }
        return sb.ToString();
    }
    private async Task<string> ListAuditLog(string userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = (GroupSavingsApi.Data.GroupSavingsDbContext)scope.ServiceProvider.GetService(typeof(GroupSavingsApi.Data.GroupSavingsDbContext));
        var userGuid = Guid.Parse(userId);
        var logs = db.AuditLogs.Where(a => a.UserId == userGuid).OrderByDescending(a => a.Timestamp).ToList();
        if (!logs.Any()) return "No audit log entries found.";
        var sb = new StringBuilder();
        sb.AppendLine("Your Audit Log:");
        int idx = 1;
        foreach (var a in logs)
        {
            sb.AppendLine($"{idx}. [{a.Action}] {a.EntityType} {a.EntityId} ({a.Timestamp:d})");
            idx++;
        }
        return sb.ToString();
    }
    private async Task<string> ListReports(string userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = (GroupSavingsApi.Data.GroupSavingsDbContext)scope.ServiceProvider.GetService(typeof(GroupSavingsApi.Data.GroupSavingsDbContext));
        var sb = new StringBuilder();

        // By User
        var byUser = db.Contributions.Where(c => !c.IsDeleted)
            .GroupBy(c => c.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(x => x.Amount) }).ToList();
        sb.AppendLine("Contributions by User:");
        foreach (var u in byUser)
        {
            var user = db.Users.FirstOrDefault(x => x.Id == u.UserId);
            sb.AppendLine($"- {user?.Email ?? u.UserId.ToString()}: {u.Total:C}");
        }
        sb.AppendLine();

        // By Session
        var bySession = db.Contributions.Where(c => !c.IsDeleted)
            .GroupBy(c => c.GroupSessionId)
            .Select(g => new { GroupSessionId = g.Key, Total = g.Sum(x => x.Amount) }).ToList();
        sb.AppendLine("Contributions by Session:");
        foreach (var s in bySession)
        {
            var session = db.GroupSessions.FirstOrDefault(x => x.Id == s.GroupSessionId);
            sb.AppendLine($"- Session {s.GroupSessionId.ToString().Substring(0, 8)}: {s.Total:C}");
        }
        sb.AppendLine();
        return sb.ToString();
    }
    private string GetHelpText()
    {
        return "This is the Group Savings Bot. Use the menu to access all features. For help, contact support@example.com.";
    }


    private async Task SendMessage(long chatId, string text)
    {
        using var client = new HttpClient();
        var payload = new
        {
            chat_id = chatId,
            text = text
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        await client.PostAsync(ApiBase + "sendMessage", content);
    }
}
