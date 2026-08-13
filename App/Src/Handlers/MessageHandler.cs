using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Interfaces.Handlers;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace Kozma.net.Src.Handlers;

public partial class MessageHandler(IMemoryCache cache) : IMessageHandler
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

    [GeneratedRegex(@"\bkozma\b", RegexOptions.IgnoreCase)]
    private static partial Regex KozmaRegex();
}
