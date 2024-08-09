using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Handlers;
using Kozma.net.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kozma.net;

public class Bot : IBot
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly ICommandHandler _commandHandler;

    public Bot(IConfigFactory configFactory, ICommandHandler commandHandler)
    {
        _config = configFactory.GetConfig();
        _commandHandler = commandHandler;

        DiscordSocketConfig config = new()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _client.Log += Log;
        _client.SlashCommandExecuted += _commandHandler.HandleCommandAsync;
    }

    public async Task StartAsync(ServiceProvider provider)
    {
        await _client.LoginAsync(TokenType.Bot, _config.GetValue<string>("botToken"));
        await _client.StartAsync();
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
