using Discord;

namespace Kozma.net.Factories;

public interface IEmbedFactory
{
    EmbedBuilder GetEmbed(string title);
    Embed GetAndBuildEmbed(string title);
}
