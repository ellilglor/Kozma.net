using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Kozma.net.Src.Logging;

public partial class Logger(IBot bot,
    IConfiguration config,
    IEmbedHandler embedHandler,
    IRateLimitHandler rateLimitHandler,
    IUserService userService,
    ICommandService commandService) : IBotLogger
{
    private readonly DiscordSocketClient _client = bot.GetClient();

    public void Log(LogLevel level, string message) =>
        Console.WriteLine($"{level.Color()}[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\u001b[0m {message}");

    public async Task LogAsync(string? message = null, Embed? embed = null, bool pingOwner = false)
    {
        if (string.IsNullOrEmpty(message) && embed is null) return;
        if (await _client.GetChannelAsync(config.GetValue<ulong>("ids:botLogsChannel")) is not IMessageChannel channel) return;
        var msg = pingOwner ? string.Join(" ", $"<@{config.GetValue<ulong>("ids:owner")}>", message) : message;

        await channel.SendMessageAsync(msg, embed: embed);
    }

    public async Task HandlePostInteractionAsync(ICommandInfo command, IInteractionContext context, IResult result)
    {
        var location = context.Interaction.IsDMInteraction ? "DM" : context.Guild.Name;

        if (!result.IsSuccess)
        {
            await HandleErrorAsync(command.Name, context.Interaction, (ExecuteResult)result, location);
            return;
        }

        if (AdminCommandsRegex().IsMatch(command.Name)) return;
        if (context.User.Id == config.GetValue<ulong>("ids:owner")) return;

        switch (context.Interaction.Type)
        {
            case InteractionType.ApplicationCommand: await HandleCommandAsync(command.Name, context.Interaction, location); break;
            case InteractionType.MessageComponent: await HandleComponentAsync((IComponentInteraction)context.Interaction, location); break;
            default: return;
        }
    }

    public EmbedBuilder GetLogEmbed(string title, uint color) =>
        embedHandler.GetBasicEmbed(title).WithColor(color).WithCurrentTimestamp();

    private async Task HandleCommandAsync(string command, IDiscordInteraction interaction, string location)
    {
        await SaveInteractionAsync(interaction.User.Id, interaction.User.Username, command, GameRegex().IsMatch(command));

        var fields = new List<EmbedFieldBuilder>
        {
            embedHandler.CreateField("Action", $"/{command}"),
            embedHandler.CreateField(location, interaction.UserLocale),
        };

        var embed = GetLogEmbed(string.Empty, Colors.Default)
            .WithDescription(interaction.Data is SocketSlashCommandData data && data.Options.Count > 0 ? ExtractOptions(data.Options) : string.Empty)
            .WithAuthor(new EmbedAuthorBuilder().WithName(interaction.User.Username).WithIconUrl(interaction.User.GetDisplayAvatarUrl()))
            .WithFooter(new EmbedFooterBuilder().WithText($"ID: {interaction.User.Id}"))
            .WithFields(fields);

        Log(LogLevel.Command, $"{interaction.User.Username} used /{command} in {location}");
        await LogAsync(embed: embed.Build());
    }

    private async Task HandleComponentAsync(IComponentInteraction interaction, string location)
    {
        switch (interaction.Data.CustomId)
        {
            case ComponentIds.UnboxBase + ComponentIds.UnboxAgain: await SaveInteractionAsync(interaction.User.Id, interaction.User.Username, CommandIds.Unbox, isCommand: false); break;
            case ComponentIds.ClearMessages: await SaveInteractionAsync(interaction.User.Id, interaction.User.Username, CommandIds.Clear, isCommand: true); break;
            case ComponentIds.ShardSweepReload: await SaveInteractionAsync(interaction.User.Id, interaction.User.Username, CommandIds.ShardSweeper, isCommand: false); break;
        }

        Log(LogLevel.Button, $"{interaction.User.Username} used {interaction.Data.CustomId} in {location}");
    }

    private async Task SaveInteractionAsync(ulong id, string user, string command, bool isCommand)
    {
        await userService.UpdateOrSaveUserAsync(id, user, isCommand, command);
        await commandService.UpdateOrSaveCommandAsync(command, isCommand);
    }

    private async Task HandleErrorAsync(string command, IDiscordInteraction interaction, ExecuteResult result, string location)
    {
        var interactionName = interaction.Type switch
        {
            InteractionType.ApplicationCommand => (interaction as SocketCommandBase)?.CommandName,
            InteractionType.MessageComponent => (interaction as IComponentInteraction)?.Data.CustomId,
            _ => command
        };
        if (string.IsNullOrEmpty(interactionName)) interactionName = command;

        var stackTrace = result.Exception.InnerException?.StackTrace;
        var fields = new List<EmbedFieldBuilder>
        {
            embedHandler.CreateField("Type", interaction.Type.ToString()),
            embedHandler.CreateField("Location", location),
            embedHandler.CreateField("Locale", interaction.UserLocale),
        };
        if (interaction.Data is SocketSlashCommandData data && data.Options.Count > 0) fields.Add(embedHandler.CreateField("Options", ExtractOptions(data.Options)));

        var errorEmbed = GetLogEmbed($"Error while executing __{interactionName}__ for __{interaction.User.Username}__", Colors.Error)
            .WithDescription(string.Join("\n\n", result.Exception.InnerException?.Message, stackTrace?.Substring(0, Math.Min(stackTrace.Length, ExtendedDiscordConfig.MaxEmbedDescChars))))
            .WithFooter(new EmbedFooterBuilder().WithText($"ID: {interaction.User.Id}"))
            .WithFields(fields);

        await LogAsync(embed: errorEmbed.Build(), pingOwner: true);
        await InformUserAsync(interaction, result);
    }

    private static string ExtractOptions(IReadOnlyCollection<SocketSlashCommandDataOption> options) =>
        string.Join("\n", options.Select(o => $"- **{o.Name}**: {o.Value}"));

    private async Task InformUserAsync(IDiscordInteraction interaction, ExecuteResult result)
    {
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
            .WithDescription(string.Join("\n\n", description, $"<@{config.GetValue<ulong>("ids:owner")}> has been notified"))
            .WithColor(Colors.Error);

        Log(LogLevel.Error, result.Exception.InnerException?.Message ?? description);
        await interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = userEmbed.Build();
            msg.Components = new ComponentBuilder().Build();
        });
    }

    public async Task HandleDiscordLog(LogMessage msg)
    {
        var message = $"{msg.Source}\t{msg.Message}";
        if (!string.IsNullOrEmpty(msg.Message)) Log(LogLevel.Discord, message);

        if (msg.Severity == LogSeverity.Critical || msg.Severity == LogSeverity.Error) await LogAsync(message, pingOwner: true);
        if (msg.Message != null && msg.Message.Contains("Rate limit triggered", StringComparison.OrdinalIgnoreCase)) rateLimitHandler.SetRateLimit(msg.Message);
    }

    [GeneratedRegex(@"(pricecheck|stats|test|update|shardsweeper)")] // TODO remove before merging!!
    private static partial Regex AdminCommandsRegex();

    [GeneratedRegex(@"(unbox|punch|shardsweeper)")]
    private static partial Regex GameRegex();
}
