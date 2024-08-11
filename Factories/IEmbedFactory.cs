using Discord;

namespace Kozma.net.Factories;

public interface IEmbedFactory
{
    EmbedBuilder GetEmbed(string title);
    Embed GetAndBuildEmbed(string title);
    EmbedFieldBuilder CreateField(string name, string value, bool inline = true);
}
