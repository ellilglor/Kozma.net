using Discord;
using Discord.WebSocket;

namespace Kozma.net.Src.Interfaces;

public interface IBot
{
    DiscordSocketClient Client { get; }
    long ReadyTimeStamp { get; }
    Task StartAsync();
    Task UpdateActivityAsync(string activity, ActivityType type);
    void Dispose();
}
