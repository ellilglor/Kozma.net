using Discord;

namespace Kozma.net.Src.Handlers;

public interface IRoleHandler
{
    Task GiveRoleAsync(IGuildUser user, ulong roleId);
    Task RemoveRoleAsync(IGuildUser user, ulong roleId);
    Task HandleTradeCooldownAsync(IMessage message, ulong roleId);
    Task CheckTradeMessagesAsync();
    Task CheckExpiredMutesAsync();
    Task<bool> CheckOutdatedMutesAsync();
}
