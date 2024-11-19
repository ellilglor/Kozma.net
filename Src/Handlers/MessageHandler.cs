using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Logging;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Handlers;

public class MessageHandler(IBot bot, IConfiguration config, IBotLogger logger) : IMessageHandler
{
    public async Task InitializeAsync()
    {
        bot.GetClient().MessageReceived += HandleMessageAsync;
        await Task.CompletedTask;
    }

    private async Task HandleMessageAsync(SocketMessage message)
    {
        //if (message.Author.Id != config.GetValue<ulong>("ids:ownerId")) return;
        var channelType = message.Channel.GetChannelType();
        if (channelType is null || channelType != ChannelType.Text && channelType != ChannelType.News) return;

        var channel = (ITextChannel)message.Channel;
        if (channel.GuildId.Equals(config.GetValue<ulong>("ids:serverId"))) await HandleKbpMessageAsync(message);
        else if (channel.GuildId.Equals(653349356459786240)) await HandleHavenMessageAsync(message);
    }

    private async Task HandleKbpMessageAsync(SocketMessage message)
    {
        await Task.CompletedTask;
    }

    private static async Task HandleHavenMessageAsync(SocketMessage message)
    {
        switch (message.Channel.Id)
        {
            case 1059194248894885968 when message.Author.IsWebhook:
                if (message.Author is IWebhookUser webhook && webhook.WebhookId == 1059194506248978432) await message.Channel.SendMessageAsync($"<@&1059195232018772031> The following has been posted:\n{message.Content}");
                break;
        }
    }
}
