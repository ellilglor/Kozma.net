using Discord;

namespace Kozma.net.Src.Handlers;

public interface IEmbedHandler
{
    EmbedBuilder GetEmbed(string title);
    EmbedBuilder GetBasicEmbed(string title);
    Embed GetAndBuildEmbed(string title);
    EmbedFieldBuilder CreateField(string name, string value, bool isInline = true);
    EmbedFieldBuilder CreateEmptyField(bool isInline = true);
}
