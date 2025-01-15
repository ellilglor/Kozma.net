using Discord;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Extensions;

namespace Kozma.net.Src.Handlers;

public class EmbedHandler(IBot bot) : IEmbedHandler
{
    public EmbedBuilder GetEmbed(string title)
    {
        return GetBasicEmbed(title)
            .WithFooter(
                new EmbedFooterBuilder()
                    .WithText($"Thank you for using {bot.Client.CurrentUser.Username} bot!")
                    .WithIconUrl(bot.Client.CurrentUser.GetDisplayAvatarUrl())
            );
    }

    public EmbedBuilder GetBasicEmbed(string title)
    {
        return new EmbedBuilder
        {
            Title = title.Substring(0, Math.Min(title.Length, ExtendedDiscordConfig.MaxEmbedTitleChars)),
            Color = Colors.Default
        };
    }

    public Embed GetAndBuildEmbed(string title) =>
        GetEmbed(title).Build();

    public EmbedFieldBuilder CreateField(string name, string value, bool isInline = true) =>
        new EmbedFieldBuilder().WithName(name).WithValue(value).WithIsInline(isInline);

    public EmbedFieldBuilder CreateEmptyField(bool isInline = true) =>
        new EmbedFieldBuilder().WithName(Emotes.Empty).WithValue(Emotes.Empty).WithIsInline(isInline);
}
