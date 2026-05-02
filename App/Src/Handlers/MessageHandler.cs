using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace Kozma.net.Src.Handlers;

public partial class MessageHandler(IMemoryCache cache, IRoleHandler roleHandler) : IMessageHandler
{
    private const string _cachekey = "Kozma_Mentioned";
    private static readonly Random _random = new();

    public async Task HandleMessageAsync(SocketMessage rawMessage)
    {
        if (rawMessage is not SocketUserMessage message) return;
        var channelType = message.Channel.GetChannelType();
        if (channelType is null || channelType != ChannelType.Text && channelType != ChannelType.News) return;

        var channel = (ITextChannel)message.Channel;
        switch (channel.GuildId)
        {
            case Data.Constants.Ids.Server: await HandleKbpMessageAsync(message, message.Channel.Id); break;
            case Data.Constants.Ids.Haven: await HandleHavenMessageAsync(message); break;
        }

        if (KozmaRegex().IsMatch(message.Content) && _random.Next(4) == 0)
        {
            try
            {
                await message.AddReactionAsync(new Emote(Data.Constants.Ids.LogoEmote, "kbplogo"));
            }
            catch { } // in case no permission to react
        }
    }

    private async Task HandleKbpMessageAsync(SocketUserMessage message, ulong channelId)
    {
        if (message.Author.IsWebhook && channelId == Data.Constants.ChannelIds.Announcements)
        {
            await message.CrosspostAsync();
            return;
        }

        if (message.Author.IsBot)
        {
            if (channelId == Data.Constants.ChannelIds.Market)
                await message.CrosspostAsync();

            return;
        }

        switch (channelId)
        {
            case Data.Constants.ChannelIds.WTS: 
                await roleHandler.HandleTradeCooldownAsync(message, Data.Constants.RoleIds.WTS);
                await CheckTradePostWarnings(message, isWtsChannel: true);
                break;
            case Data.Constants.ChannelIds.WTB: 
                await roleHandler.HandleTradeCooldownAsync(message, Data.Constants.RoleIds.WTB);
                await CheckTradePostWarnings(message, isWtsChannel: false);
                break;
            case Data.Constants.ChannelIds.General when message.MentionedUsers.Any(user => user.Id == Data.Constants.Ids.Kozma) && !cache.TryGetValue(_cachekey, out int _):
                await message.Channel.SendFileAsync(filePath: Path.Combine("Src", "Assets", "hello-there.gif"));
                cache.Set(_cachekey, 0, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) });
                break;
            default: break;
        }
    }

    private static async Task HandleHavenMessageAsync(IMessage message)
    {
        if (message.Channel.Id == Data.Constants.ChannelIds.HavenListings && message.Author.IsWebhook)
            await message.Channel.SendMessageAsync($"{MentionUtils.MentionRole(Data.Constants.RoleIds.HavenListings)} The following has been posted:\n{message.Content}");
    }

    private static async Task CheckTradePostWarnings(SocketUserMessage message, bool isWtsChannel)
    {
        await WarnIfWrongContentAsync(message, isWtsChannel);
        await WarnIfContentTooLongAsync(message);
        await WarnIfIncorrectFormat(message);
    }

    private static async Task WarnIfWrongContentAsync(SocketUserMessage message, bool isWtsChannel)
    {
        if (isWtsChannel && !message.Content.Contains("wtb", StringComparison.OrdinalIgnoreCase) && !message.Content.Contains("buying", StringComparison.OrdinalIgnoreCase) && !message.Content.Contains("looking for", StringComparison.OrdinalIgnoreCase)
            && !(message.Content.Contains("lf", StringComparison.OrdinalIgnoreCase) && !message.Content.Contains("wolf", StringComparison.OrdinalIgnoreCase))) return;
        if (!isWtsChannel && !message.Content.Contains("wts", StringComparison.OrdinalIgnoreCase) && !message.Content.Contains("selling", StringComparison.OrdinalIgnoreCase)) return;

        await ReplyAndDeleteAsync(message, $"It looks like you're selling or buying items in the incorrect channel.\nPlease edit your post through the {Format.Code("/tradepostedit")} command.\nIf this is not the case, you can ignore this warning.");
    }

    private static async Task WarnIfContentTooLongAsync(SocketUserMessage message)
    {
        var count = NewLineRegex().Matches(message.Content).Count;
        if (count < 15) return;

        await ReplyAndDeleteAsync(message, $"Your message is too long, check the pinned messages for the channel guidelines.\nPlease edit your post through the {Format.Code("/tradepostedit")} command.\nIgnoring this warning may result in your post being deleted and a timeout.");
    }

    private static async Task WarnIfIncorrectFormat(SocketUserMessage message)
    {
        if (!message.Content.StartsWith('#')) return;

        await ReplyAndDeleteAsync(message, $"Your message appears to be incorrectly formatted.\ncheck the pinned messages for the channel guidelines.\nPlease edit your post through the {Format.Code("/tradepostedit")} command.");
    }

    private static async Task ReplyAndDeleteAsync(SocketUserMessage message, string msg)
    {
        var response = await message.ReplyAsync(msg);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(1));
            await response.DeleteAsync();
        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    [GeneratedRegex(@"\bkozma\b", RegexOptions.IgnoreCase)]
    private static partial Regex KozmaRegex();

    [GeneratedRegex("\n")]
    private static partial Regex NewLineRegex();
}
