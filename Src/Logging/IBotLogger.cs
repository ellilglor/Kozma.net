﻿using Discord;
using Discord.Interactions;
using Kozma.net.Src.Enums;

namespace Kozma.net.Src.Logging;

public interface IBotLogger
{
    void Log(LogLevel level, string message);
    Task LogAsync(string? message = null, Embed? embed = null, bool pingOwner = false);
    Task HandlePostInteractionAsync(ICommandInfo command, IInteractionContext context, IResult result);
    Task HandleDiscordLog(LogMessage msg);
}
