using Discord.WebSocket;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Logging;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Handlers;

public class RoleHandler(IBot bot, IConfiguration config, IBotLogger logger) : IRoleHandler
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
        if (user.Roles.Any(r => r.Id == config.GetValue<ulong>("ids:adminId") || r.Id == config.GetValue<ulong>("ids:modId"))) return;

        await GiveRoleAsync(user, roleId);
    }

    private SocketGuild GetGuild()
    {
       return _client.Guilds.FirstOrDefault(g => g.Id.Equals(config.GetValue<ulong>("ids:serverId")))!;
    }
}
