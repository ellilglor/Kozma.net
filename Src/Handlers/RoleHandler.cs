﻿using Discord;
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
        if (user.Roles.Any(r => r.Id == config.GetValue<ulong>("ids:admin") || r.Id == config.GetValue<ulong>("ids:mod"))) return;

        await MuteUserAsync(user, message, roleId);
    }

    public async Task CheckTradeMessagesAsync()
    {
        var offlineMutesCheckInterval = 3;
        var currentDate = DateTime.UtcNow;
        var taskName = "offlineMutes";
        var task = await taskService.GetTaskAsync(taskName);

        if (task is null)
        {
            await logger.LogAsync($"failed to fetch {taskName} from db", pingOwner: true);
            return;
        }
        if (task.UpdatedAt.AddHours(offlineMutesCheckInterval) > currentDate) return;

        logger.Log(LogLevel.Moderation, "Checking if people need to be muted");

        var guild = GetGuild();
        if (!guild.HasAllMembers) await guild.DownloadUsersAsync(); // Assure the users will be in the cache

        var editRole = await guild.GetRoleAsync(config.GetValue<ulong>("ids:editRole"));
        var users = guild.Users.Where(u => u.Roles.Any(r => r.Id == editRole.Id)).ToList(); // Should be empty but exists just in case
        foreach (var user in users)
        {
            await RemoveRoleAsync(user, config.GetValue<ulong>("ids:editRole"));
        }

        await CheckMessagesAsync(guild, config.GetValue<ulong>("ids:wtsChannel"), config.GetValue<ulong>("ids:wtsRole"), currentDate);
        await CheckMessagesAsync(guild, config.GetValue<ulong>("ids:wtbChannel"), config.GetValue<ulong>("ids:wtbRole"), currentDate);
        await taskService.UpdateTaskAsync(taskName);
    }

    public async Task CheckExpiredMutesAsync()
    {
        logger.Log(LogLevel.Moderation, "Checking expired mutes");
        var sellMutes = await userService.GetAndDeleteExpiredMutesAsync<SellMute>();
        var buyMutes = await userService.GetAndDeleteExpiredMutesAsync<BuyMute>();

        if (!sellMutes.Any() && !buyMutes.Any()) return; // Both are empty
        var guild = GetGuild();
        if (!guild.HasAllMembers) await guild.DownloadUsersAsync(); // Assure the users will be in the cache

        await RemoveExpiredMutesAsync(guild, config.GetValue<ulong>("ids:wtsRole"), sellMutes);
        await RemoveExpiredMutesAsync(guild, config.GetValue<ulong>("ids:wtbRole"), buyMutes);
    }

    private async Task RemoveExpiredMutesAsync<T>(SocketGuild guild, ulong roleId, IEnumerable<T> mutes) where T : Mute
    {
        foreach (var m in mutes)
        {
            if (guild.GetUser(ulong.Parse(m.Id)) is not IGuildUser user) continue; // User left the server
            await RemoveRoleAsync(user, roleId);
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
            if (user.Roles.Any(r => r.Id == roleId || r.Id == config.GetValue<ulong>("ids:admin") || r.Id == config.GetValue<ulong>("ids:mod"))) continue;

            await MuteUserAsync(user, message, roleId);
        }
    }

    private async Task MuteUserAsync(IGuildUser user, IMessage message, ulong roleId)
    {
        bool success;
        if (roleId == config.GetValue<ulong>("ids:wtsRole")) success = await userService.SaveMuteAsync(user.Id, message.CreatedAt.DateTime, () => new SellMute() { Id = user.Id.ToString(), Name = user.Username });
        else success = await userService.SaveMuteAsync(user.Id, message.CreatedAt.DateTime, () => new BuyMute() { Id = user.Id.ToString(), Name = user.Username });

        if (!success) await logger.LogAsync($"- {(roleId == config.GetValue<ulong>("ids:wtsRole") ? "WTS" : "WTB")} {MentionUtils.MentionUser(user.Id)} is already in the database", pingOwner: true);
        else await GiveRoleAsync(user, roleId);
    }
}
