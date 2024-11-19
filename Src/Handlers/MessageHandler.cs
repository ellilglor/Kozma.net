using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Handlers;

public class MessageHandler(IBot bot, IConfiguration config, IRoleHandler roleHandler) : IMessageHandler
{
    public async Task InitializeAsync()
    {
        bot.GetClient().MessageReceived += HandleMessageAsync;
        await Task.CompletedTask;
    }

    private async Task HandleMessageAsync(SocketMessage rawMessage)
    {
        if (rawMessage is not SocketUserMessage message) return;
        var channelType = message.Channel.GetChannelType();
        if (channelType is null || channelType != ChannelType.Text && channelType != ChannelType.News) return;

        var channel = (ITextChannel)message.Channel;
        if (channel.GuildId.Equals(config.GetValue<ulong>("ids:serverId"))) await HandleKbpMessageAsync(message);
        else if (channel.GuildId.Equals(653349356459786240)) await HandleHavenMessageAsync(message);
    }

    private async Task HandleKbpMessageAsync(SocketUserMessage message)
    {
        if (message.Author.IsBot)
        {
            if (message.Channel.Id.Equals(config.GetValue<ulong>("ids:marketChannelId"))) await message.CrosspostAsync();
        }
        else
        {
            if (message.Channel.Id.Equals(config.GetValue<ulong>("ids:wtsChannelId"))) await roleHandler.HandleTradeCooldownAsync(message, config.GetValue<ulong>("ids:wtsRoleId"));
            else if (message.Channel.Id.Equals(config.GetValue<ulong>("ids:wtbChannelId"))) await roleHandler.HandleTradeCooldownAsync(message, config.GetValue<ulong>("ids:wtbRoleId"));
        }
    }

    private static async Task HandleHavenMessageAsync(SocketUserMessage message)
    {
        switch (message.Channel.Id)
        {
            case 1059194248894885968 when message.Author.IsWebhook: // Listings channel that get cross-posted from main server.
                if (message.Author is IWebhookUser webhook && webhook.WebhookId == 1059194506248978432) await message.Channel.SendMessageAsync($"<@&1059195232018772031> The following has been posted:\n{message.Content}");
                break;
        }
    }
}
