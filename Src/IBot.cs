using Discord.WebSocket;

namespace Kozma.net.Src;

public interface IBot
{
    Task StartAsync();
    DiscordSocketClient GetClient();
    long GetReadyTimestamp();
}
