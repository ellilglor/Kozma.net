using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Kozma.net;

public interface IBot
{
    Task StartAsync();
    DiscordSocketClient GetClient();
    long GetReadyTimestamp();
}
