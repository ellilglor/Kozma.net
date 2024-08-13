﻿using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Factories;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Kozma.net.Handlers;

public class InteractionHandler(IBot bot, IConfigFactory configFactory, IEmbedFactory embedFactory, IServiceProvider services, InteractionService handler) : IInteractionHandler
{
    private readonly DiscordSocketClient client = bot.GetClient();
    private readonly IConfiguration config = configFactory.GetConfig();

    public async Task InitializeAsync()
    {
        client.Ready += ReadyAsync;

        await handler.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        client.InteractionCreated += HandleInteractionAsync;
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
            var maintenanceEmbed = embedFactory.GetAndBuildEmbed("The bot is currently being worked on.\nPlease try again later.");
            await interaction.RespondAsync(embed: maintenanceEmbed, ephemeral: true);
            return;
        }

        await interaction.DeferAsync(ephemeral: true);

        // TODO: check if banned from server

        try
        {
            var context = new SocketInteractionContext(client, interaction);

            var result = await handler.ExecuteCommandAsync(context, services);

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnknownCommand:
                        await interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embedFactory.GetAndBuildEmbed($"It looks like this command is missing!"));
                        break;
                    default:
                        Console.WriteLine(result.Error);
                        break;
                }
            }
        } catch (Exception ex) 
        {
            Console.WriteLine(ex.ToString());
            // TODO: Let user know crash happened
        }
    }
}
