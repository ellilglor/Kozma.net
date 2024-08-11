using Discord;
using Discord.WebSocket;
using Kozma.net.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kozma.net;

public class Bot : IBot
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;

    public Bot(IConfigFactory configFactory)
    {
        _config = configFactory.GetConfig();

        DiscordSocketConfig config = new()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _client.Log += Log;
    }

    public async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _config.GetValue<string>("botToken"));
        await _client.StartAsync();
    }

    public DiscordSocketClient GetClient()
    {
        return _client;
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
