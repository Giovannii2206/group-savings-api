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
                // View groups
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
                var userId = state.UserId;
                if (!Guid.TryParse(userId, out var userGuid))
                {
                    await SendMessage(chatId, "You are not registered. Please register first.");
                    return Ok();
                }
                var groupMemberships = db.GroupMembers.Include(gm => gm.Group)
                    .Where(gm => gm.UserId == userGuid && !gm.IsDeleted && !gm.Group.IsDeleted)
                    .Select(gm => gm.Group).ToList();
                if (groupMemberships.Count == 0)
                {
                    await SendMessage(chatId, "You are not a member of any groups.");
                }
                else
                {
                    var msg = "Your groups:\n" + string.Join("\n", groupMemberships.Select(g => $"{g.Name} (ID: {g.Id})"));
                    await SendMessage(chatId, msg);
                }
                await SendMessage(chatId, "Type the group ID to manage sessions, contributions, invitations, or type /menu to return to main menu.");
                state.Step = SessionStep.ViewingGroups;
            }
            else if (text == "4")
            {
                await SendMessage(chatId, "Viewing sessions. Please enter the group ID to view its sessions:");
                state.Step = SessionStep.ViewingSessions;
            }
            else if (text == "5")
            {
                await SendMessage(chatId, "Viewing savings goals. Please enter the group session ID to view/set goals:");
                state.Step = SessionStep.ViewingSavingsGoals;
            }
            else if (text == "6")
            {
                await SendMessage(chatId, "Viewing notifications:");
                state.Step = SessionStep.ViewingNotifications;
            }
            else if (text == "7")
            {
                await SendMessage(chatId, "Viewing reports:");
                state.Step = SessionStep.ViewingReports;
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
        else if (state.Step == SessionStep.ViewingGroups)
        {
            if (text == "/menu")
            {
                await SendMessage(chatId, "Returning to main menu.");
                state.Step = SessionStep.MainMenu;
                return Ok();
            }
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
            if (!Guid.TryParse(text, out var groupId))
            {
                await SendMessage(chatId, "Invalid group ID. Type the group ID to manage or /menu to return.");
                return Ok();
            }
            var group = db.Groups.FirstOrDefault(g => g.Id == groupId && !g.IsDeleted);
            if (group == null)
            {
                await SendMessage(chatId, "Group not found.");
                return Ok();
            }
            await SendMessage(chatId, $"Managing group: {group.Name}\nOptions:\n1. View sessions\n2. Create session\n3. Invite member\n4. View contributions\n5. Make contribution\nType option number, or /menu to return.");
            state.UserId = state.UserId; // keep
            state.Step = SessionStep.ViewingSessions;
            state.TempGroupId = groupId.ToString();
        }
        else if (state.Step == SessionStep.ViewingSessions)
        {
            if (text == "/menu")
            {
                await SendMessage(chatId, "Returning to main menu.");
                state.Step = SessionStep.MainMenu;
                return Ok();
            }
            var groupId = Guid.Parse(state.TempGroupId);
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
            if (text == "1")
            {
                // View sessions
                var sessions = db.GroupSessions.Where(s => s.GroupId == groupId && !s.IsDeleted).ToList();
                if (sessions.Count == 0)
                {
                    await SendMessage(chatId, "No sessions found for this group.");
                }
                else
                {
                    var msg = "Sessions:\n" + string.Join("\n", sessions.Select(s => $"{s.Id}: Target {s.TargetAmount} by {(s.TargetDate == null ? "N/A" : s.TargetDate.ToString("yyyy-MM-dd"))}, Status: {s.Status}"));
                    await SendMessage(chatId, msg);
                }
                await SendMessage(chatId, "Type session ID to manage, or /menu to return.");
                state.Step = SessionStep.ViewingSessions;
            }
            else if (text == "2")
            {
                await SendMessage(chatId, "Creating new session. Enter target amount:");
                state.Step = SessionStep.CreatingSession;
            }
            else if (text == "3")
            {
                await SendMessage(chatId, "Enter email of member to invite:");
                state.Step = SessionStep.InvitingToSession;
            }
            else if (text == "4")
            {
                // View contributions
                var sessions = db.GroupSessions.Where(s => s.GroupId == groupId && !s.IsDeleted).ToList();
                var msg = "Contributions by session:\n";
                foreach (var session in sessions)
                {
                    var contribs = db.Contributions.Where(c => c.GroupSessionId == session.Id && !c.IsDeleted).ToList();
                    msg += $"Session {session.Id}: {contribs.Sum(c => c.Amount)} total, {contribs.Count} contributions\n";
                }
                await SendMessage(chatId, msg);
                await SendMessage(chatId, "Type session ID to view details, or /menu to return.");
                state.Step = SessionStep.ViewingContributions;
            }
            else if (text == "5")
            {
                await SendMessage(chatId, "Enter session ID to contribute to:");
                state.Step = SessionStep.MakingContribution;
            }
            else if (Guid.TryParse(text, out var sessionId))
            {
                await SendMessage(chatId, $"Managing session {sessionId}. Options:\n1. View savings goals\n2. Set savings goal\n3. View contributions\n4. Make contribution\nType option number, or /menu to return.");
                state.TempSessionId = sessionId.ToString();
                state.Step = SessionStep.ViewingSavingsGoals;
            }
            else
            {
                await SendMessage(chatId, "Invalid option. Type option number, session ID, or /menu to return.");
            }
        }
        else if (state.Step == SessionStep.CreatingSession)
        {
            // expects: target amount
            if (!decimal.TryParse(text, out var targetAmount))
            {
                await SendMessage(chatId, "Invalid amount. Enter a numeric value:");
                return Ok();
            }
            await SendMessage(chatId, "Enter target date (YYYY-MM-DD):");
            state.TempTargetAmount = text;
            state.Step = SessionStep.CreatingSession + 1000; // hack: next step
        }
        else if ((int)state.Step == (int)SessionStep.CreatingSession + 1000)
        {
            // expects: target date
            if (!DateTime.TryParse(text, out var targetDate))
            {
                await SendMessage(chatId, "Invalid date. Enter as YYYY-MM-DD:");
                return Ok();
            }
            var groupId = Guid.Parse(state.TempGroupId);
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
            var session = new GroupSavingsApi.Models.GroupSession
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                TargetAmount = decimal.Parse(state.TempTargetAmount),
                TargetDate = targetDate,
                Status = "Active",
                IsDeleted = false
            };
            db.GroupSessions.Add(session);
            db.SaveChanges();
            await SendMessage(chatId, $"Session created: {session.Id}");
            state.Step = SessionStep.ViewingSessions;
        }
        else if (state.Step == SessionStep.InvitingToSession)
        {
            var email = text.Trim();
            var groupId = Guid.Parse(state.TempGroupId);
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
            var userId = state.UserId;
            if (!Guid.TryParse(userId, out var inviterId))
            {
                await SendMessage(chatId, "You are not registered. Please register first.");
                return Ok();
            }
            var invitation = new GroupSavingsApi.Models.GroupInvitation
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                InviterId = inviterId,
                InviteeEmail = email,
                SentAt = DateTime.UtcNow,
                Accepted = false,
                IsDeleted = false
            };
            db.GroupInvitations.Add(invitation);
            db.SaveChanges();
            await SendMessage(chatId, $"Invitation sent to {email}.");
            state.Step = SessionStep.ViewingSessions;
        }
        else if (state.Step == SessionStep.ViewingContributions)
        {
            if (text == "/menu")
            {
                await SendMessage(chatId, "Returning to main menu.");
                state.Step = SessionStep.MainMenu;
                return Ok();
            }
            if (!Guid.TryParse(text, out var sessionId))
            {
                await SendMessage(chatId, "Invalid session ID. Type a valid session ID or /menu to return.");
                return Ok();
            }
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
            var contribs = db.Contributions.Where(c => c.GroupSessionId == sessionId && !c.IsDeleted).ToList();
            if (contribs.Count == 0)
            {
                await SendMessage(chatId, "No contributions found for this session.");
            }
            else
            {
                var msg = "Contributions:\n" + string.Join("\n", contribs.Select(c => $"{c.UserId}: {c.Amount} on {c.Date.ToShortDateString()} ({c.Type})"));
                await SendMessage(chatId, msg);
            }
            state.Step = SessionStep.ViewingContributions;
        }
        else if (state.Step == SessionStep.MakingContribution)
        {
            if (!Guid.TryParse(text, out var sessionId))
            {
                await SendMessage(chatId, "Invalid session ID. Enter a valid session ID:");
                return Ok();
            }
            await SendMessage(chatId, "Enter amount to contribute:");
            state.TempSessionId = sessionId.ToString();
            state.Step = SessionStep.MakingContribution + 1000;
        }
        else if ((int)state.Step == (int)SessionStep.MakingContribution + 1000)
        {
            if (!decimal.TryParse(text, out var amount))
            {
                await SendMessage(chatId, "Invalid amount. Enter a numeric value:");
                return Ok();
            }
            var sessionId = Guid.Parse(state.TempSessionId);
            var userId = state.UserId;
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
            var contrib = new GroupSavingsApi.Models.Contribution
            {
                Id = Guid.NewGuid(),
                GroupSessionId = sessionId,
                UserId = Guid.Parse(userId),
                Amount = amount,
                Date = DateTime.UtcNow,
                Type = "Manual",
                IsDeleted = false
            };
            db.Contributions.Add(contrib);
            db.SaveChanges();
            await SendMessage(chatId, $"Contribution of {amount} added to session {sessionId}.");
            state.Step = SessionStep.ViewingSessions;
        }
        else if (state.Step == SessionStep.ViewingSavingsGoals)
        {
            if (text == "/menu")
            {
                await SendMessage(chatId, "Returning to main menu.");
                state.Step = SessionStep.MainMenu;
                return Ok();
            }
            var sessionId = Guid.Parse(state.TempSessionId);
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
            if (text == "1")
            {
                var goals = db.SavingsGoals.Where(g => g.GroupSessionId == sessionId && !g.IsDeleted).ToList();
                if (goals.Count == 0)
                {
                    await SendMessage(chatId, "No savings goals found for this session.");
                }
                else
                {
                    var msg = "Savings Goals:\n" + string.Join("\n", goals.Select(g => $"{g.Name}: Target {g.TargetAmount}, Current {g.CurrentAmount}, Target Date: {(g.TargetDate == null ? "N/A" : g.TargetDate.ToString("yyyy-MM-dd"))}"));
                    await SendMessage(chatId, msg);
                }
                state.Step = SessionStep.ViewingSavingsGoals;
            }
            else if (text == "2")
            {
                await SendMessage(chatId, "Enter name for new savings goal:");
                state.Step = SessionStep.SettingSavingsGoal;
            }
            else if (text == "3")
            {
                var contribs = db.Contributions.Where(c => c.GroupSessionId == sessionId && !c.IsDeleted).ToList();
                if (contribs.Count == 0)
                {
                    await SendMessage(chatId, "No contributions found for this session.");
                }
                else
                {
                    var msg = "Contributions:\n" + string.Join("\n", contribs.Select(c => $"{c.UserId}: {c.Amount} on {c.Date.ToShortDateString()} ({c.Type})"));
                    await SendMessage(chatId, msg);
                }
                state.Step = SessionStep.ViewingSavingsGoals;
            }
            else if (text == "4")
            {
                await SendMessage(chatId, "Enter amount to contribute:");
                state.Step = SessionStep.MakingContribution;
            }
            else
            {
                await SendMessage(chatId, "Invalid option. Type option number or /menu to return.");
            }
        }
        else if (state.Step == SessionStep.SettingSavingsGoal)
        {
            state.TempGoalName = text;
            await SendMessage(chatId, "Enter target amount for the goal:");
            state.Step = SessionStep.SettingSavingsGoal + 1000;
        }
        else if ((int)state.Step == (int)SessionStep.SettingSavingsGoal + 1000)
        {
            if (!decimal.TryParse(text, out var targetAmount))
            {
                await SendMessage(chatId, "Invalid amount. Enter a numeric value:");
                return Ok();
            }
            await SendMessage(chatId, "Enter target date (YYYY-MM-DD):");
            state.TempTargetAmount = text;
            state.Step = SessionStep.SettingSavingsGoal + 2000;
        }
        else if ((int)state.Step == (int)SessionStep.SettingSavingsGoal + 2000)
        {
            if (!DateTime.TryParse(text, out var targetDate))
            {
                await SendMessage(chatId, "Invalid date. Enter as YYYY-MM-DD:");
                return Ok();
            }
            var sessionId = Guid.Parse(state.TempSessionId);
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
            var goal = new GroupSavingsApi.Models.SavingsGoal
            {
                Id = Guid.NewGuid(),
                GroupSessionId = sessionId,
                Name = state.TempGoalName,
                TargetAmount = decimal.Parse(state.TempTargetAmount),
                CurrentAmount = 0,
                TargetDate = targetDate,
                IsDeleted = false
            };
            db.SavingsGoals.Add(goal);
            db.SaveChanges();
            await SendMessage(chatId, $"Savings goal '{goal.Name}' created with target {goal.TargetAmount}.");
            state.Step = SessionStep.ViewingSavingsGoals;
        }
        else if (state.Step == SessionStep.ViewingNotifications)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
            var userId = state.UserId;
            if (!Guid.TryParse(userId, out var userGuid))
            {
                await SendMessage(chatId, "You are not registered. Please register first.");
                return Ok();
            }
            var notifications = db.Notifications.Where(n => n.UserId == userGuid).ToList();
            if (notifications.Count == 0)
            {
                await SendMessage(chatId, "No notifications found.");
            }
            else
            {
                var msg = "Notifications:\n" + string.Join("\n", notifications.Select(n => $"{n.CreatedAt.ToShortDateString()}: {n.Message} ({n.Type})"));
                await SendMessage(chatId, msg);
            }
            state.Step = SessionStep.MainMenu;
        }
        else if (state.Step == SessionStep.ViewingReports)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
            var userId = state.UserId;
            if (!Guid.TryParse(userId, out var userGuid))
            {
                await SendMessage(chatId, "You are not registered. Please register first.");
                return Ok();
            }
            // Simple report: total contributed by user
            var userContribs = db.Contributions.Where(c => c.UserId == userGuid && !c.IsDeleted).ToList();
            var total = userContribs.Sum(c => c.Amount);
            await SendMessage(chatId, $"Your total contributions: {total}");
            state.Step = SessionStep.MainMenu;
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
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
        // Find Telegram user
        var userState = UserStates.Values.FirstOrDefault(s => s.Step == SessionStep.CreatingGroup);
        var userIdStr = userState?.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return string.Empty;
        // Create group
        var group = new GroupSavingsApi.Models.Group
        {
            Id = Guid.NewGuid(),
            Name = groupName,
            CreatedBy = userId,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        db.Groups.Add(group);
        await db.SaveChangesAsync();
        // Assign creator as admin (find admin role)
        var adminRole = await db.GroupRoles.FirstOrDefaultAsync(r => r.Name.ToLower() == "admin");
        if (adminRole == null)
        {
            adminRole = new GroupSavingsApi.Models.GroupRole { Name = "admin", Description = "Group admin" };
            db.GroupRoles.Add(adminRole);
            await db.SaveChangesAsync();
        }
        var gm = new GroupSavingsApi.Models.GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId,
            RoleId = adminRole.Id,
            JoinedAt = DateTime.UtcNow,
            TotalContributed = 0,
            IsDeleted = false
        };
        db.GroupMembers.Add(gm);
        await db.SaveChangesAsync();
        return group.Id.ToString();
    }

    private async Task<string> JoinGroup(string groupCode)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GroupSavingsApi.Data.GroupSavingsDbContext>();
        // Find Telegram user
        var userState = UserStates.Values.FirstOrDefault(s => s.Step == SessionStep.JoiningGroup);
        var userIdStr = userState?.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return string.Empty;
        // Find group by code (ID)
        if (!Guid.TryParse(groupCode, out var groupId))
            return string.Empty;
        var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);
        if (group == null)
            return string.Empty;
        // Assign as member (find member role)
        var memberRole = await db.GroupRoles.FirstOrDefaultAsync(r => r.Name.ToLower() == "member");
        if (memberRole == null)
        {
            memberRole = new GroupSavingsApi.Models.GroupRole { Name = "member", Description = "Group member" };
            db.GroupRoles.Add(memberRole);
            await db.SaveChangesAsync();
        }
        // Prevent duplicate membership
        var alreadyMember = await db.GroupMembers.AnyAsync(gm => gm.GroupId == group.Id && gm.UserId == userId && !gm.IsDeleted);
        if (alreadyMember)
            return group.Id.ToString();
        var gm = new GroupSavingsApi.Models.GroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId,
            RoleId = memberRole.Id,
            JoinedAt = DateTime.UtcNow,
            TotalContributed = 0,
            IsDeleted = false
        };
        db.GroupMembers.Add(gm);
        await db.SaveChangesAsync();
        return group.Id.ToString();
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
    JoiningGroup,
    ViewingGroups,
    CreatingSession,
    JoiningSession,
    ViewingSessions,
    InvitingToSession,
    MakingContribution,
    ViewingContributions,
    SettingSavingsGoal,
    ViewingSavingsGoals,
    ViewingNotifications,
    ViewingReports
}

public class UserSession
{
    public string UserId { get; set; }
    public SessionStep Step { get; set; }
    public string? TempGroupId { get; set; }
    public string? TempSessionId { get; set; }
    public string? TempTargetAmount { get; set; }
    public string? TempGoalName { get; set; }
}
