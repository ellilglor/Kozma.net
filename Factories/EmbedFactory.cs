using Discord;
using Discord.WebSocket;

namespace Kozma.net.Factories;

public class EmbedFactory : IEmbedFactory
{
    private readonly DiscordSocketClient _client;
    private readonly UInt32 _defaultColor;

    public EmbedFactory(IBot bot)
    {
        _client = bot.GetClient();
        _defaultColor = Convert.ToUInt32("29D0FF", 16);
    }

    public Embed GetAndBuildEmbed(string title)
    {
        return GetEmbed(title).Build();
    }

    public EmbedBuilder GetEmbed(string title)
    {
        return new EmbedBuilder
        {
            Title = title,
            Color = _defaultColor,
            Footer = new EmbedFooterBuilder()
                .WithText($"Thank you for using {_client.CurrentUser.Username} bot!")
                .WithIconUrl(_client.CurrentUser.GetDisplayAvatarUrl())
        };
    }

    public EmbedFieldBuilder CreateField(string name, string value, bool inline = true)
    {
        return new EmbedFieldBuilder().WithName(name).WithValue(value).WithIsInline(inline);
    }
}
