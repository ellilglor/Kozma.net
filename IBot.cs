using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Kozma.net;

public interface IBot
{
    Task StartAsync(ServiceProvider provider);
    DiscordSocketClient GetClient();
}
