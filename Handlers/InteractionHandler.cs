using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Factories;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Kozma.net.Handlers;

public class InteractionHandler : IInteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly IEmbedFactory _embedFactory;
    private readonly IServiceProvider _services;
    private readonly InteractionService _handler;

    public InteractionHandler(IBot bot, IConfigFactory configFactory, IEmbedFactory embedFactory, IServiceProvider services, InteractionService handler)
    {
        _client = bot.GetClient();
        _config = configFactory.GetConfig();
        _embedFactory = embedFactory;
        _services = services;
        _handler = handler;
    }

    public async Task InitializeAsync()
    {
        _client.Ready += ReadyAsync;

        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        _client.InteractionCreated += HandleInteractionAsync;
    }

    private async Task ReadyAsync()
    {
        // TODO: register commands
        await Task.CompletedTask;
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (interaction.User.Id != _config.GetValue<ulong>("ids:ownerId"))
        {
            var maintenanceEmbed = _embedFactory.GetAndBuildEmbed("The bot is currently being worked on.\nPlease try again later.");
            await interaction.RespondAsync(embed: maintenanceEmbed, ephemeral: true);
            return;
        }

        await interaction.DeferAsync(ephemeral: true);

        // TODO: check if banned from server

        try
        {
            var context = new SocketInteractionContext(_client, interaction);

            var result = await _handler.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnknownCommand:
                        await interaction.ModifyOriginalResponseAsync(msg => msg.Embed = _embedFactory.GetAndBuildEmbed($"It looks like this command is missing!"));
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
