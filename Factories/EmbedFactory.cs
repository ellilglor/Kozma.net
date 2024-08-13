using Discord;
using Discord.WebSocket;

namespace Kozma.net.Factories;

public class EmbedFactory(IBot bot) : IEmbedFactory
{
    private readonly DiscordSocketClient client = bot.GetClient();
    private readonly UInt32 defaultColor = Convert.ToUInt32("29D0FF", 16);

    public Embed GetAndBuildEmbed(string title)
    {
        return GetEmbed(title).Build();
    }

    public EmbedBuilder GetEmbed(string title)
    {
        return new EmbedBuilder
        {
            Title = title,
            Color = defaultColor,
            Footer = new EmbedFooterBuilder()
                .WithText($"Thank you for using {client.CurrentUser.Username} bot!")
                .WithIconUrl(client.CurrentUser.GetDisplayAvatarUrl())
        };
    }

    public EmbedFieldBuilder CreateField(string name, string value, bool inline = true)
    {
        return new EmbedFieldBuilder().WithName(name).WithValue(value).WithIsInline(inline);
    }
}
