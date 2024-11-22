using Discord;
using Discord.WebSocket;

namespace Kozma.net.Src;

public interface IBot
{
    Task StartAsync();
    Task UpdateActivityAsync(string activity, ActivityType type);
    DiscordSocketClient GetClient();
    long GetReadyTimestamp();
}
