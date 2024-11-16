using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Enums;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Kozma.net.Handlers;

public class InteractionHandler(IBot bot, IConfiguration config, IEmbedHandler embedHandler, IServiceProvider services, InteractionService handler) : IInteractionHandler
{
    private readonly DiscordSocketClient _client = bot.GetClient();

    public async Task InitializeAsync()
    {
        _client.Ready += ReadyAsync;

        await handler.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        _client.InteractionCreated += HandleInteractionAsync;
        handler.InteractionExecuted += HandlePostInteractionAsync;
    }

    private async Task ReadyAsync()
    {
        // TODO: register commands
        await Task.CompletedTask;
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (interaction.User.Id != config.GetValue<ulong>("ids:ownerId"))
        {
            var maintenanceEmbed = embedHandler.GetAndBuildEmbed("The bot is currently being worked on.\nPlease try again later.");
            await interaction.RespondAsync(embed: maintenanceEmbed, ephemeral: true);
            return;
        }

        await interaction.DeferAsync(ephemeral: true);

        // TODO: check if banned from server

        var context = new SocketInteractionContext(_client, interaction);
        await handler.ExecuteCommandAsync(context, services);
    }

    private async Task HandlePostInteractionAsync(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            await HandleErrorAsync(command.Name, context, result);
        }
    }

    private async Task HandleErrorAsync(string command, IInteractionContext context, IResult result)
    {
        var description = result.Error switch
        {
            InteractionCommandError.UnmetPrecondition => $"Unmet Precondition.",
            InteractionCommandError.UnknownCommand => $"It looks like this command is missing.",
            InteractionCommandError.BadArgs => "Invalid number of arguments given.",
            InteractionCommandError.Exception => $"An exception was thrown during execution.",
            InteractionCommandError.Unsuccessful => "Command could not be executed.",
            InteractionCommandError.ConvertFailed => "Failed to convert one or more parameters.",
            InteractionCommandError.ParseFailed => "Failed to parse the command.",
            _ => $"Unknown reason."
        };

        var interactionName = context.Interaction.Type switch
        {
            InteractionType.ApplicationCommand => (context.Interaction as SocketSlashCommand)?.CommandName,
            InteractionType.MessageComponent => (context.Interaction as SocketMessageComponent)?.Data.CustomId,
            _ => command
        };
        if (string.IsNullOrEmpty(interactionName)) interactionName = command;

        var error = (ExecuteResult)result;
        var stackTrace = error.Exception.InnerException?.StackTrace;
        var channel = (ISocketMessageChannel)await _client.GetChannelAsync(config.GetValue<ulong>("ids:botLogsChannelId"));
        var color = embedHandler.ConvertEmbedColor(EmbedColor.Error);
        var fields = new List<EmbedFieldBuilder>
        {
            embedHandler.CreateField("User", context.User.Username),
            embedHandler.CreateField("UserId", context.User.Id.ToString()),
            embedHandler.CreateField("Locale", context.Interaction.UserLocale),
            embedHandler.CreateField("Command", interactionName),
            embedHandler.CreateField("Type", context.Interaction.Type.ToString()),
            embedHandler.CreateField("Location", context.Interaction.IsDMInteraction ? "DM" : context.Guild.Name),
        };
        if (context.Interaction.Data is SocketSlashCommandData data && data.Options.Count > 0) fields.Add(embedHandler.CreateField("Options", string.Join("\n", data.Options.Select(o => $"{o.Name}: {o.Value}"))));

        var errorEmbed = embedHandler.GetBasicEmbed($"Error while executing __{interactionName}__ for __{context.User.Username}__")
            .WithDescription(string.Join("\n\n", 
                error.Exception.InnerException?.Message,
                stackTrace?.Length < (int)DiscordCharLimit.EmbedDesc ? stackTrace : stackTrace?.Substring(0, (int)DiscordCharLimit.EmbedDesc)))
            .WithFields(fields)
            .WithColor(color);
        await channel.SendMessageAsync($"<@{config.GetValue<ulong>("ids:ownerId")}>", embed: errorEmbed.Build());

        var userEmbed = embedHandler.GetEmbed("Something went wrong while executing this command.")
            .WithDescription(string.Join("\n\n", description, $"<@{config.GetValue<ulong>("ids:ownerId")}> has been notified"))
            .WithColor(color);
        await context.Interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = userEmbed.Build();
            msg.Components = new ComponentBuilder().Build();
        });
    }
}
