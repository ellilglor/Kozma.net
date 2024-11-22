using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Enums;

namespace Kozma.net.Src.Handlers;

public class InteractionHandler(IBot bot, IBotLogger logger, IConfiguration config, IEmbedHandler embedHandler, IServiceProvider services, InteractionService handler) : IInteractionHandler
{
    private readonly DiscordSocketClient _client = bot.GetClient();

    public async Task InitializeAsync()
    {
        await handler.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        handler.InteractionExecuted += logger.HandlePostInteractionAsync;
    }

    public async Task RegisterCommandsAsync()
    {
        // TODO: register commands
        await Task.CompletedTask;
    }

    public async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (interaction.User.Id != config.GetValue<ulong>("ids:owner"))
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
