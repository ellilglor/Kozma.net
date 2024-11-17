using Discord.Interactions;
using Discord;
using Kozma.net.Enums;

namespace Kozma.net.Logging;

public interface IBotLogger
{
    void Log(LogColor level, string message);
    Task LogAsync(string? message = null, Embed? embed= null);
    Task HandlePostInteractionAsync(ICommandInfo command, IInteractionContext context, IResult result);
}
