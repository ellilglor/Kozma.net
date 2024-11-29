using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Data.Classes;

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
            await interaction.RespondAsync(embed: embedHandler.GetAndBuildEmbed("The bot is currently being worked on.\nPlease try again later."), ephemeral: true);
            logger.Log(LogLevel.Info, interaction.User.Username);
            return;
        }

        await interaction.DeferAsync(ephemeral: true);

        if (interaction.GuildId != config.GetValue<ulong>("ids:server") && interaction.User.Id != config.GetValue<ulong>("ids:owner"))
        {
            var guild = _client.GetGuild(config.GetValue<ulong>("ids:server"));
            var isBanned = await guild.GetBanAsync(interaction.User.Id) != null;

            if (isBanned)
            {
                var embed = embedHandler.GetBasicEmbed("You are banned from the Kozma's Backpack Discord server and are therefore prohibited from using this bot.").WithColor(Colors.Error);
                await interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
                return;
            }
        }

        var context = new SocketInteractionContext(_client, interaction);
        await handler.ExecuteCommandAsync(context, services);
    }
}
