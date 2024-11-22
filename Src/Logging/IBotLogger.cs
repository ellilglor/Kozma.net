using Discord.Interactions;
using Discord;
using Kozma.net.Src.Enums;

namespace Kozma.net.Src.Logging;

public interface IBotLogger
{
    void Initialize();
    void Log(LogColor level, string message);
    Task LogAsync(string? message = null, Embed? embed = null, bool pingOwner = false);
    EmbedBuilder GetLogEmbed(string title, EmbedColor color);
    Task HandlePostInteractionAsync(ICommandInfo command, IInteractionContext context, IResult result);
}
