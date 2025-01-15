using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Kozma.net.Src.Handlers;

public class InteractionHandler(IBot bot,
    IBotLogger logger,
    IConfiguration config,
    IEmbedHandler embedHandler,
    ITradeLogService tradeLogService,
    IServiceProvider services,
    InteractionService service) : IInteractionHandler
{
    public async Task InitializeAsync()
    {
        await service.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        service.InteractionExecuted += logger.HandlePostInteractionAsync;
    }

    public async Task RegisterCommandsAsync()
    {
        await service.RegisterCommandsGloballyAsync();

        // Guild specific commands have [DontAutoRegister] attribute so they can be registered separately
        var kozmaCommands = service.Modules
            .Where(x => (x.IsSlashGroup && x.IsTopLevelGroup || !x.IsSubModule) && x.DontAutoRegister)
            .ToArray();

        await service.AddModulesToGuildAsync(config.GetValue<ulong>("ids:server"), deleteMissing: true, kozmaCommands);

        logger.Log(LogLevel.Discord, "Commands have been registered");
    }

    public async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        /*if (interaction.User.Id != config.GetValue<ulong>("ids:owner"))
        {
            await interaction.RespondAsync(embed: embedHandler.GetAndBuildEmbed("The bot is currently being worked on.\nPlease try again later."), ephemeral: true);
            logger.Log(LogLevel.Info, interaction.User.Username);
            return;
        }*/

        await interaction.DeferAsync(ephemeral: true);

        if (interaction.GuildId != config.GetValue<ulong>("ids:server") && interaction.User.Id != config.GetValue<ulong>("ids:owner"))
        {
            var guild = bot.Client.GetGuild(config.GetValue<ulong>("ids:server"));
            var isBanned = await guild.GetBanAsync(interaction.User.Id) != null;

            if (isBanned) // :)
            {
                var embed = embedHandler.GetBasicEmbed("You are banned from the Kozma's Backpack Discord server and are therefore prohibited from using this bot.").WithColor(Colors.Error);
                await interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
                return;
            }
        }

        var tryLater = interaction.Type switch
        {
            Discord.InteractionType.ApplicationCommand when tradeLogService.LogsAreBeingReset => interaction is SocketSlashCommand command && command.CommandName.Equals(CommandIds.FindLogs, StringComparison.Ordinal),
            Discord.InteractionType.MessageComponent when tradeLogService.LogsAreBeingReset => interaction is SocketMessageComponent component && component.Data.CustomId.Contains(ComponentIds.FindLogsBase, StringComparison.Ordinal),
            _ => false
        };

        if (tryLater)
        {
            await interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embedHandler.GetAndBuildEmbed("Logs are being updated, please try again in a few minutes."));
            return;
        }

        var context = new SocketInteractionContext(bot.Client, interaction);
        await service.ExecuteCommandAsync(context, services);
    }
}
