using Discord.WebSocket;

namespace Kozma.net.Src.Handlers;

public interface IRoleHandler
{
    Task GiveRoleAsync(SocketGuildUser user, ulong roleId);
    Task RemoveRoleAsync(SocketGuildUser user, ulong roleId);
    Task HandleTradeCooldownAsync(SocketUserMessage message, ulong roleId);
}
