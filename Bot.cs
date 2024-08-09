using Discord;
using Discord.WebSocket;
using Kozma.net.Services;
using Microsoft.Extensions.Configuration;

namespace Kozma.net;

public class Bot : IBot
{
    private readonly DiscordSocketClient _client;
    private readonly IConfigFactory _configFactory;

    public Bot(IConfigFactory configFactory)
    {
        _configFactory = configFactory;

        DiscordSocketConfig config = new()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _client.Log += Log;
    }

    public async Task StartAsync()
    {
        var config = _configFactory.GetConfig();

        await _client.LoginAsync(TokenType.Bot, config.GetValue<string>("botToken"));
        await _client.StartAsync();
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
