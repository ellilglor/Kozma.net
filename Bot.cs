using Discord;
using Discord.WebSocket;
using Kozma.net.Factories;
using Microsoft.Extensions.Configuration;

namespace Kozma.net;

public class Bot : IBot
{
    private readonly DiscordSocketClient client;
    private readonly IConfiguration config;

    public Bot(IConfigFactory configFactory)
    {
        config = configFactory.GetConfig();

        DiscordSocketConfig intents = new()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
        };

        client = new DiscordSocketClient(intents);
        client.Log += Log;
    }

    public async Task StartAsync()
    {
        await client.LoginAsync(TokenType.Bot, config.GetValue<string>("botToken"));
        await client.StartAsync();
    }

    public DiscordSocketClient GetClient()
    {
        return client;
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
