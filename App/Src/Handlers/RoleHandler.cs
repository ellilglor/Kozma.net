using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Models.Entities;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Handlers;

public class RoleHandler(IBot bot, IConfiguration config, IBotLogger logger, IUserService userService, ITaskService taskService) : IRoleHandler
{
    public async Task GiveRoleAsync(IGuildUser user, ulong roleId)
    {
        var role = await GetGuild().GetRoleAsync(roleId);
        await user.AddRoleAsync(role);
        logger.Log(LogLevel.Moderation, $"{role.Name} was given to {user.Username}");
    }

    public async Task RemoveRoleAsync(IGuildUser user, ulong roleId)
    {
        var role = await GetGuild().GetRoleAsync(roleId);
        await user.RemoveRoleAsync(role);
        logger.Log(LogLevel.Moderation, $"{role.Name} was removed from {user.Username}");
    }

    public async Task HandleTradeCooldownAsync(IMessage message, ulong roleId)
    {
        if (message.Author is not SocketGuildUser user) return;
        if (user.Roles.Any(r => r.Id == config.GetValue<ulong>("ids:roles:admin") || r.Id == config.GetValue<ulong>("ids:roles:mod"))) return;

        await MuteUserAsync(user, message, roleId);
    }

    public async Task CheckTradeMessagesAsync()
    {
        var offlineMutesCheckInterval = 15;
        var currentDate = DateTime.UtcNow;
        var taskName = "offlineMutes";
        var task = await taskService.GetTaskAsync(taskName);

        if (task is null)
        {
            await logger.LogAsync($"failed to fetch {taskName} from db", pingOwner: true);
            return;
        }
        if (task.UpdatedAt.AddMinutes(offlineMutesCheckInterval) > currentDate) return;

        logger.Log(LogLevel.Moderation, "Checking if people need to be muted");

        var guild = GetGuild();
        if (!guild.HasAllMembers) await guild.DownloadUsersAsync(); // Assure the users will be in the cache

        var editRoleId = config.GetValue<ulong>("ids:roles:edit");
        var users = guild.Users.Where(u => u.Roles.Any(r => r.Id == editRoleId)); // Should be empty but exists just in case
        foreach (var user in users)
        {
            await RemoveRoleAsync(user, editRoleId);
        }

        await CheckMessagesAsync(guild, config.GetValue<ulong>("ids:channels:wts"), config.GetValue<ulong>("ids:roles:wts"), currentDate);
        await CheckMessagesAsync(guild, config.GetValue<ulong>("ids:channels:wtb"), config.GetValue<ulong>("ids:roles:wtb"), currentDate);
        await taskService.UpdateTaskAsync(taskName);
    }

    public async Task CheckExpiredMutesAsync()
    {
        logger.Log(LogLevel.Moderation, "Checking expired mutes");
        var mutes = await userService.GetAndDeleteExpiredMutesAsync();

        if (!mutes.Any()) return; // No mutes expired => no need to continue
        var guild = GetGuild();
        if (!guild.HasAllMembers) await guild.DownloadUsersAsync(); // Assure the users will be in the cache

        foreach (var mute in mutes)
        {
            if (guild.GetUser(mute.UserId) is not IGuildUser user) continue; // User left the server
            await RemoveRoleAsync(user, config.GetValue<ulong>($"ids:roles:{(mute.IsWtb ? "wtb" : "wts")}"));
        }
    }

    private SocketGuild GetGuild() =>
        bot.Client.GetGuild(config.GetValue<ulong>("ids:server"));

    private async Task CheckMessagesAsync(SocketGuild guild, ulong channelId, ulong roleId, DateTime d)
    {
        if (guild.GetChannel(channelId) is not IMessageChannel channel) return;
        var messages = await channel.GetMessagesAsync(25).FlattenAsync();

        foreach (var message in messages)
        {
            if (d > message.CreatedAt.DateTime.AddHours(config.GetValue<double>("timers:slowmodeHours"))) break;
            if (message.Author.IsBot || message.Author is not SocketGuildUser user) continue;
            if (user.Roles.Any(r => r.Id == roleId || r.Id == config.GetValue<ulong>("ids:roles:admin") || r.Id == config.GetValue<ulong>("ids:roles:mod"))) continue;

            await MuteUserAsync(user, message, roleId);
        }
    }

    private async Task MuteUserAsync(SocketGuildUser user, IMessage message, ulong roleId)
    {
        var isWtb = roleId == config.GetValue<ulong>("ids:roles:wtb");
        var success = await userService.SaveMuteAsync(user.Id, user.Username, isWtb, message.CreatedAt.DateTime);

        if (!success) await logger.LogAsync($"- {(isWtb ? "WTB" : "WTS")} {MentionUtils.MentionUser(user.Id)} is already in the database", pingOwner: true);
        else await GiveRoleAsync(user, roleId);
    }

    public async Task<bool> CheckOutdatedMutesAsync()
    {
        try
        {
            logger.Log(LogLevel.Moderation, "Checking for outdated mutes");
            var mutes = await userService.GetMutesAsync();
            var guild = GetGuild();

            await CheckForOutdatedMutesAsync(guild, mutes, isWtb: true);
            await CheckForOutdatedMutesAsync(guild, mutes, isWtb: false);
        }
        catch (Exception ex)
        {
            await logger.LogAsync(ex.Message, pingOwner: true);
            return false;
        }

        return true;
    }

    private async Task CheckForOutdatedMutesAsync(SocketGuild guild, IEnumerable<Mute> mutes, bool isWtb)
    {
        var roleId = config.GetValue<ulong>($"ids:roles:{(isWtb ? "wtb" : "wts")}");
        var users = guild.Users.Where(u => u.Roles.Any(r => r.Id == roleId));
        var outdated = users.Where(u => !mutes.Any(m => m.IsWtb == isWtb && m.UserId == u.Id));

        // Users that have the role while not in the database
        foreach (var user in outdated)
        {
            await RemoveRoleAsync(user, roleId);
        }
    }
}
