using Discord;
using Discord.WebSocket;

namespace Kozma.net.Src;

public class Bot : IBot, IDisposable
{
    public DiscordSocketClient Client { get; private set; }
    public long ReadyTimeStamp { get; private set; }
    private bool _disposed;

    public Bot()
    {
        DiscordSocketConfig intents = new()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
        };

        Client = new DiscordSocketClient(intents);
        ReadyTimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
    }

    public async Task StartAsync()
    {
        await Client.LoginAsync(TokenType.Bot, DotNetEnv.Env.GetString("botToken"));
        await Client.StartAsync();
    }

    public async Task UpdateActivityAsync(string activity, ActivityType type) =>
        await Client.SetActivityAsync(new Game(activity, type));

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Client?.Dispose();
            }

            _disposed = true;
        }
    }
}
