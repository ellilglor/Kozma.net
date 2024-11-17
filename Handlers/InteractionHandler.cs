using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Logging;
using Kozma.net.Enums;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Kozma.net.Handlers;

public class InteractionHandler(IBot bot, IBotLogger logger, IConfiguration config, IEmbedHandler embedHandler, IServiceProvider services, InteractionService handler) : IInteractionHandler
{
    private readonly DiscordSocketClient _client = bot.GetClient();

    public async Task InitializeAsync()
    {
        _client.Ready += ReadyAsync;

        await handler.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        _client.InteractionCreated += HandleInteractionAsync;
        handler.InteractionExecuted += logger.HandlePostInteractionAsync;
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
            logger.Log(LogColor.Info, interaction.User.Username);
            return;
        }

        await interaction.DeferAsync(ephemeral: true);

        // TODO: check if banned from server

        var context = new SocketInteractionContext(_client, interaction);
        await handler.ExecuteCommandAsync(context, services);
    }
}
