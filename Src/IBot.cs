using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Kozma.net.Src;

public interface IBot
{
    Task StartAsync();
    DiscordSocketClient GetClient();
    long GetReadyTimestamp();
}
