using Discord;
using Discord.WebSocket;
using Kozma.net.Enums;

namespace Kozma.net.Factories;

public class EmbedFactory(IBot bot) : IEmbedFactory
{
    private readonly DiscordSocketClient _client = bot.GetClient();

    public Embed GetAndBuildEmbed(string title)
    {
        return GetEmbed(title).Build();
    }

    public EmbedBuilder GetEmbed(string title)
    {
        return new EmbedBuilder
        {
            Title = title,
            Color = ConvertEmbedColor(EmbedColor.Default),
            Footer = new EmbedFooterBuilder()
                .WithText($"Thank you for using {_client.CurrentUser.Username} bot!")
                .WithIconUrl(_client.CurrentUser.GetDisplayAvatarUrl())
        };
    }

    public EmbedFieldBuilder CreateField(string name, string value, bool inline = true)
    {
        return new EmbedFieldBuilder().WithName(name).WithValue(value).WithIsInline(inline);
    }

    public uint ConvertEmbedColor(EmbedColor color)
    {
        return (uint)color;
    } 
}
