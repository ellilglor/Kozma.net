using Discord.WebSocket;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Models.Entities;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Handlers;

public class RoleHandler(IBot bot, IConfiguration config, IBotLogger logger, IUserService userService) : IRoleHandler
{
    private readonly DiscordSocketClient _client = bot.GetClient();

    public async Task GiveRoleAsync(SocketGuildUser user, ulong roleId)
    {
        var role = GetGuild().GetRole(roleId);
        await user.AddRoleAsync(role);
        logger.Log(LogColor.Moderation, $"{role.Name} was given to {user.Username}");
    }

    public async Task RemoveRoleAsync(SocketGuildUser user, ulong roleId)
    {
        var role = GetGuild().GetRole(roleId);
        await user.RemoveRoleAsync(role);
        logger.Log(LogColor.Moderation, $"{role.Name} was removed from {user.Username}");
    }

    public async Task HandleTradeCooldownAsync(SocketUserMessage message, ulong roleId)
    {
        if (message.Author is not SocketGuildUser user) return;
        if (user.Roles.Any(r => r.Id == config.GetValue<ulong>("ids:admin") || r.Id == config.GetValue<ulong>("ids:mod"))) return;

        bool success;
        if (roleId == config.GetValue<ulong>("ids:wtsRole")) success = await userService.SaveMuteAsync(user.Id, message.CreatedAt.DateTime, () => new SellMute() { Id = user.Id.ToString(), Name = user.Username });
        else success = await userService.SaveMuteAsync(user.Id, message.CreatedAt.DateTime, () => new BuyMute() { Id = user.Id.ToString(), Name = user.Username });

        if (!success) await logger.LogAsync($"{(roleId == config.GetValue<ulong>("ids:wtsRole") ? "WTS" : "WTB")} - <@{config.GetValue<ulong>("ids:owner")}> <@{user.Id}> is already in the database!");
        else await GiveRoleAsync(user, roleId);
    }

    private SocketGuild GetGuild()
    {
       return _client.Guilds.FirstOrDefault(g => g.Id.Equals(config.GetValue<ulong>("ids:server")))!;
    }
}
