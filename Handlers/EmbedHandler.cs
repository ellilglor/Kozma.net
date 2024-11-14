using Discord;
using Discord.WebSocket;
using Kozma.net.Enums;

namespace Kozma.net.Handlers;

public class EmbedHandler(IBot bot) : IEmbedHandler
{
    private readonly DiscordSocketClient _client = bot.GetClient();

    public EmbedBuilder GetEmbed(string title)
    {
        return GetBasicEmbed(title)
            .WithFooter(
                new EmbedFooterBuilder()
                    .WithText($"Thank you for using {_client.CurrentUser.Username} bot!")
                    .WithIconUrl(_client.CurrentUser.GetDisplayAvatarUrl())
            );
    }

    public EmbedBuilder GetBasicEmbed(string title)
    {
        return new EmbedBuilder
        {
            Title = title.Length > (int)DiscordCharLimit.EmbedTitle ? title.Substring(0, (int)DiscordCharLimit.EmbedTitle) : title,
            Color = ConvertEmbedColor(EmbedColor.Default)
        };
    }

    public Embed GetAndBuildEmbed(string title)
    {
        return GetEmbed(title).Build();
    }

    public EmbedFieldBuilder CreateField(string name, string value, bool inline = true)
    {
        return new EmbedFieldBuilder().WithName(name).WithValue(value).WithIsInline(inline);
    }

    public EmbedFieldBuilder CreateEmptyField(bool inline = true)
    {
        return new EmbedFieldBuilder().WithName("\u200b").WithValue("\u200b").WithIsInline(inline);
    }

    public uint ConvertEmbedColor(EmbedColor color)
    {
        return (uint)color;
    }
}
