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

    public void Log(LogLevel level, string message)
    {
        var color = level switch
        {
            LogLevel.Command => "\u001b[34m",
            LogLevel.Button => "\u001b[36m",
            LogLevel.Moderation => "\u001b[35m",
            LogLevel.Info => "\u001b[33m",
            LogLevel.Discord => "\u001b[90m",
            LogLevel.Special => "\u001b[32m",
            LogLevel.Error => "\u001b[31m",
            _ => "\u001b[37m"
        };

        Console.WriteLine($"{color}[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\u001b[0m {message}");
    }

    public async Task LogAsync(string? message = null, Embed? embed = null, bool pingOwner = false)
    {
        if (string.IsNullOrEmpty(message) && embed is null) return;
        if (await _client.GetChannelAsync(config.GetValue<ulong>("ids:botLogsChannel")) is not ISocketMessageChannel channel) return;
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
            case InteractionType.MessageComponent: await HandleComponentAsync((SocketMessageComponent)context.Interaction, location); break;
            default: await Task.CompletedTask; break;
        }
    }

    public EmbedBuilder GetLogEmbed(string title, uint color) =>
        embedHandler.GetBasicEmbed(title).WithColor(color).WithCurrentTimestamp();

    private async Task HandleCommandAsync(string command, IDiscordInteraction interaction, string location)
    {
        await SaveInteractionAsync(interaction.User.Id, interaction.User.Username, command, GameRegex().IsMatch(command), isUnbox: command == "unbox");

        var desc = string.Empty;
        if (interaction.Data is SocketSlashCommandData data && data.Options.Count > 0) desc = string.Join("\n", data.Options.Select(o => $"- **{o.Name}**: {o.Value}"));

        var fields = new List<EmbedFieldBuilder>
        {
            embedHandler.CreateField("Action", $"/{command}"),
            embedHandler.CreateField(location, interaction.UserLocale),
        };

        var embed = GetLogEmbed(string.Empty, Colors.Default)
            .WithDescription(desc)
            .WithFields(fields)
            .WithAuthor(new EmbedAuthorBuilder().WithName(interaction.User.Username).WithIconUrl(interaction.User.GetDisplayAvatarUrl()))
            .WithFooter(new EmbedFooterBuilder().WithText($"ID: {interaction.User.Id}"));

        Log(LogLevel.Command, $"{interaction.User.Username} used /{command} in {location}");
        await LogAsync(embed: embed.Build());
    }

    private async Task HandleComponentAsync(SocketMessageComponent interaction, string location)
    {
        switch (interaction.Data.CustomId)
        {
            case "unbox-again": await SaveInteractionAsync(interaction.User.Id, interaction.User.Username, "unbox", isCommand: false); break;
            case "clear-messages": await SaveInteractionAsync(interaction.User.Id, interaction.User.Username, "clear", isCommand: true); break;
        }

        Log(LogLevel.Button, $"{interaction.User.Username} used {interaction.Data.CustomId} in {location}");
    }

    private async Task SaveInteractionAsync(ulong id, string user, string command, bool isCommand, bool isUnbox = true)
    {
        await userService.UpdateOrSaveUserAsync(id, user, isCommand, isUnbox);
        await commandService.UpdateOrSaveCommandAsync(command, isCommand);
    }

    private async Task HandleErrorAsync(string command, IDiscordInteraction interaction, ExecuteResult result, string location)
    {
        var interactionName = interaction.Type switch
        {
            InteractionType.ApplicationCommand => (interaction as SocketSlashCommand)?.CommandName,
            InteractionType.MessageComponent => (interaction as SocketMessageComponent)?.Data.CustomId,
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
        if (interaction.Data is SocketSlashCommandData data && data.Options.Count > 0) fields.Add(embedHandler.CreateField("Options", string.Join("\n", data.Options.Select(o => $"- **{o.Name}**: {o.Value}"))));

        var errorEmbed = GetLogEmbed($"Error while executing __{interactionName}__ for __{interaction.User.Username}__", Colors.Error)
            .WithDescription(string.Join("\n\n", result.Exception.InnerException?.Message, stackTrace?.Substring(0, Math.Min(stackTrace.Length, ExtendedDiscordConfig.MaxEmbedDescChars))))
            .WithFooter(new EmbedFooterBuilder().WithText($"ID: {interaction.User.Id}"))
            .WithFields(fields);
        await LogAsync(embed: errorEmbed.Build(), pingOwner: true);

        await InformUserAsync(interaction, result);
    }

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

    public Task HandleDiscordLog(LogMessage msg)
    {
        Log(LogLevel.Discord, $"{msg.Source}\t{msg.Message}");
        // TODO logasync if error

        if (msg.Message.Contains("Rate limit triggered", StringComparison.OrdinalIgnoreCase)) rateLimitHandler.SetRateLimit(msg.Message);
        
        return Task.CompletedTask;
    }

    [GeneratedRegex(@"(pricecheck|stats|test|update)")]
    private static partial Regex AdminCommandsRegex();

    [GeneratedRegex(@"(unbox|punch)")]
    private static partial Regex GameRegex();
}
