﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src;

public class Bot : IBot, IDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly DateTime _ready;
    private bool _disposed;

    public Bot(IConfiguration config)
    {
        _config = config;

        DiscordSocketConfig intents = new()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(intents);
        _ready = DateTime.UtcNow;
    }

    public async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _config.GetValue<string>("botToken"));
        await _client.StartAsync();
    }

    public async Task UpdateActivityAsync(string activity, ActivityType type) =>
        await _client.SetActivityAsync(new Game(activity, type));

    public DiscordSocketClient GetClient() =>
        _client;

    public long GetReadyTimestamp() =>
        new DateTimeOffset(_ready).ToUnixTimeSeconds();

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
                _client?.Dispose();
            }

            _disposed = true;
        }
    }
}
