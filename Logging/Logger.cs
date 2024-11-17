using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Enums;
using Kozma.net.Handlers;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Kozma.net.Logging;

public partial class Logger(IBot bot, IConfiguration config, IEmbedHandler embedHandler) : IBotLogger
{
    private readonly DiscordSocketClient _client = bot.GetClient();

    public void Log(LogColor level, string message)
    {
        var color = level switch
        {
            LogColor.Command => "\u001b[34m",
            LogColor.Button => "\u001b[36m",
            LogColor.Moderation => "\u001b[35m",
            LogColor.Info => "\u001b[33m",
            LogColor.Error => "\u001b[31m",
            _ => "\u001b[37m"
        };

        Console.WriteLine($"{color}[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\u001b[0m {message}");
    }

    public async Task LogAsync(string? message = null, Embed? embed = null)
    {
        if (string.IsNullOrEmpty(message) && embed is null) return;
        if (await _client.GetChannelAsync(config.GetValue<ulong>("ids:botLogsChannelId")) is not ISocketMessageChannel channel) return;

        await channel.SendMessageAsync(message, embed: embed);
    }

    public async Task HandlePostInteractionAsync(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            await HandleErrorAsync(command.Name, context, result);
            return;
        }

        //if (context.User.Id == config.GetValue<ulong>("ids:ownerId")) return;
        if (context.Interaction.Type != InteractionType.ApplicationCommand) return;
        if (AdminCommandsRegex().IsMatch(command.Name)) return;


        var desc = string.Empty;
        if (context.Interaction.Data is SocketSlashCommandData data && data.Options.Count > 0) desc = string.Join("\n", data.Options.Select(o => $"- **{o.Name}**: {o.Value}"));

        var fields = new List<EmbedFieldBuilder>
        {
            embedHandler.CreateField("Action", $"/{command.Name}"),
            embedHandler.CreateField(context.Interaction.IsDMInteraction ? "DM" : context.Guild.Name, context.Interaction.UserLocale),
        };

        var embed = GetLogEmbed(string.Empty, EmbedColor.Default)
            .WithDescription(desc)
            .WithFields(fields)
            .WithAuthor(new EmbedAuthorBuilder().WithName(context.User.Username).WithIconUrl(context.User.GetDisplayAvatarUrl()))
            .WithFooter(new EmbedFooterBuilder().WithText($"ID: {context.User.Id}"));

        Log(LogColor.Command, $"{context.User.Username} used /{command.Name}");
        await LogAsync(embed: embed.Build());
    }

    private EmbedBuilder GetLogEmbed(string title, EmbedColor color)
    {
        return embedHandler.GetBasicEmbed(title)
            .WithColor((uint)color)
            .WithCurrentTimestamp();
    }

    private async Task HandleErrorAsync(string command, IInteractionContext context, IResult result)
    {
        var interactionName = context.Interaction.Type switch
        {
            InteractionType.ApplicationCommand => (context.Interaction as SocketSlashCommand)?.CommandName,
            InteractionType.MessageComponent => (context.Interaction as SocketMessageComponent)?.Data.CustomId,
            _ => command
        };
        if (string.IsNullOrEmpty(interactionName)) interactionName = command;

        var error = (ExecuteResult)result;
        var stackTrace = error.Exception.InnerException?.StackTrace;
        var fields = new List<EmbedFieldBuilder>
        {
            embedHandler.CreateField("Type", context.Interaction.Type.ToString()),
            embedHandler.CreateField("Location", context.Interaction.IsDMInteraction ? "DM" : context.Guild.Name),
            embedHandler.CreateField("Locale", context.Interaction.UserLocale),
        };
        if (context.Interaction.Data is SocketSlashCommandData data && data.Options.Count > 0) fields.Add(embedHandler.CreateField("Options", string.Join("\n", data.Options.Select(o => $"{o.Name}: {o.Value}"))));

        var errorEmbed = GetLogEmbed($"Error while executing __{interactionName}__ for __{context.User.Username}__", EmbedColor.Error)
            .WithDescription(string.Join("\n\n",
                error.Exception.InnerException?.Message,
                stackTrace?.Length < (int)DiscordCharLimit.EmbedDesc ? stackTrace : stackTrace?.Substring(0, (int)DiscordCharLimit.EmbedDesc)))
            .WithFooter(new EmbedFooterBuilder().WithText($"ID: {context.User.Id}"))
            .WithFields(fields);
        await LogAsync($"<@{config.GetValue<ulong>("ids:ownerId")}>", errorEmbed.Build());

        var description = result.Error switch
        {
            InteractionCommandError.UnmetPrecondition => "Unmet Precondition.",
            InteractionCommandError.UnknownCommand => "It looks like this command is missing.",
            InteractionCommandError.BadArgs => "Invalid number of arguments given.",
            InteractionCommandError.Exception => "An exception was thrown during execution.",
            InteractionCommandError.Unsuccessful => "Command could not be executed.",
            InteractionCommandError.ConvertFailed => "Failed to convert one or more parameters.",
            InteractionCommandError.ParseFailed => "Failed to parse the command.",
            _ => "Unknown reason."
        };
        var userEmbed = embedHandler.GetEmbed("Something went wrong while executing this command.")
            .WithDescription(string.Join("\n\n", description, $"<@{config.GetValue<ulong>("ids:ownerId")}> has been notified"))
            .WithColor((uint)EmbedColor.Error);

        Log(LogColor.Error, error.Exception.InnerException?.Message ?? description);
        await context.Interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = userEmbed.Build();
            msg.Components = new ComponentBuilder().Build();
        });
    }

    [GeneratedRegex(@"(pricecheck|stats|test|update)")]
    private static partial Regex AdminCommandsRegex();
}
