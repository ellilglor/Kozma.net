using Discord.Interactions;
using Discord;
using Kozma.net.Src.Enums;

namespace Kozma.net.Src.Logging;

public interface IBotLogger
{
    void Log(LogLevel level, string message);
    Task LogAsync(string? message = null, Embed? embed = null, bool pingOwner = false);
    EmbedBuilder GetLogEmbed(string title, uint color);
    Task HandlePostInteractionAsync(ICommandInfo command, IInteractionContext context, IResult result);
    Task HandleDiscordLog(LogMessage msg);
}
