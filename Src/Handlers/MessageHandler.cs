using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Handlers;

public class MessageHandler(IConfiguration config, IRoleHandler roleHandler) : IMessageHandler
{
    public async Task HandleMessageAsync(SocketMessage rawMessage)
    {
        if (rawMessage is not SocketUserMessage message) return;
        var channelType = message.Channel.GetChannelType();
        if (channelType is null || channelType != ChannelType.Text && channelType != ChannelType.News) return;

        var channel = (ITextChannel)message.Channel;
        if (channel.GuildId.Equals(config.GetValue<ulong>("ids:server"))) await HandleKbpMessageAsync(message, message.Channel.Id);
        else if (channel.GuildId.Equals(653349356459786240)) await HandleHavenMessageAsync(message);
    }

    private async Task HandleKbpMessageAsync(SocketUserMessage message, ulong channelId)
    {
        if (message.Author.IsBot)
        {
            if (channelId.Equals(config.GetValue<ulong>("ids:marketChannel"))) await message.CrosspostAsync();
        }
        else
        {
            if (channelId.Equals(config.GetValue<ulong>("ids:wtsChannel"))) await roleHandler.HandleTradeCooldownAsync(message, config.GetValue<ulong>("ids:wtsRole"));
            else if (channelId.Equals(config.GetValue<ulong>("ids:wtbChannel"))) await roleHandler.HandleTradeCooldownAsync(message, config.GetValue<ulong>("ids:wtbRole"));

            if (channelId.Equals(config.GetValue<ulong>("ids:wtsChannel")) || channelId.Equals(config.GetValue<ulong>("ids:wtbChannel"))) await WarnIfWrongContentAsync(message, isWtsChannel: channelId == config.GetValue<ulong>("ids:wtsChannel"));
        }
    }

    private static async Task HandleHavenMessageAsync(IMessage message)
    {
        switch (message.Channel.Id)
        {
            case 1059194248894885968 when message.Author.IsWebhook: // Listings channel that get cross-posted from main server.
                if (message.Author is IWebhookUser webhook && webhook.WebhookId == 1059194506248978432) await message.Channel.SendMessageAsync($"<@&1059195232018772031> The following has been posted:\n{message.Content}");
                break;
        }
    }

    private static async Task WarnIfWrongContentAsync(SocketUserMessage message, bool isWtsChannel)
    {
        if (isWtsChannel && (!message.Content.Contains("wtb", StringComparison.OrdinalIgnoreCase) && !message.Content.Contains("buying", StringComparison.OrdinalIgnoreCase))) return;
        if (!isWtsChannel && (!message.Content.Contains("wts", StringComparison.OrdinalIgnoreCase) && !message.Content.Contains("selling", StringComparison.OrdinalIgnoreCase))) return;

        var response = await message.ReplyAsync(
            "It looks like you're selling or buying items in the incorrect channel.\nPlease edit your message through the `/tradepostedit` command.\nIf this is not the case, you can ignore this warning.");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(1));
            await response.DeleteAsync();
        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }
}
