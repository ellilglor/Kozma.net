using Discord;
using Kozma.net.Enums;

namespace Kozma.net.Handlers;

public interface IEmbedHandler
{
    EmbedBuilder GetEmbed(string title);
    EmbedBuilder GetBasicEmbed(string title);
    Embed GetAndBuildEmbed(string title);
    EmbedFieldBuilder CreateField(string name, string value, bool inline = true);
    uint ConvertEmbedColor(EmbedColor color);
}
