﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Kozma.net.Src.Handlers;

public class InteractionHandler(IBot bot,
    IBotLogger logger,
    IConfiguration config,
    IMemoryCache cache,
    IFileReader jsonFileReader,
    IEmbedHandler embedHandler,
    ITaskHandler taskHandler,
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

        if (interaction.Type == InteractionType.ApplicationCommandAutocomplete)
        {
            await HandleAutocompleteAsync((SocketAutocompleteInteraction)interaction);
            return;
        }

        var canBeExecuted = await CheckIfCanBeExecutedAsync(interaction);
        if (!canBeExecuted) return;

        var context = new SocketInteractionContext(bot.Client, interaction);
        await service.ExecuteCommandAsync(context, services);

        // Infinite loop can randomly stop running. This should restart the method in case that happens. If case => reduce amount of checks.
        if (interaction.Type == InteractionType.ApplicationCommand) await taskHandler.CheckIfTaskHandlerIsRunningAsync();
    }

    private async Task<bool> CheckIfCanBeExecutedAsync(SocketInteraction interaction)
    {
        /*if (interaction.User.Id != config.GetValue<ulong>("ids:owner"))
        {
            await interaction.RespondAsync(embed: embedHandler.GetAndBuildEmbed("The bot is currently being worked on.\nPlease try again later."), ephemeral: true);
            logger.Log(LogLevel.Info, interaction.User.Username);
            return false;
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
                return false;
            }
        }

        var tryLater = interaction.Type switch
        {
            InteractionType.ApplicationCommand when tradeLogService.LogsAreBeingUpdated => interaction is SocketSlashCommand command && command.CommandName.Equals(CommandIds.FindLogs, StringComparison.Ordinal),
            InteractionType.MessageComponent when tradeLogService.LogsAreBeingUpdated => interaction is SocketMessageComponent component && component.Data.CustomId.Contains(ComponentIds.FindLogsBase, StringComparison.Ordinal),
            _ => false
        };

        if (tryLater)
        {
            await interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embedHandler.GetAndBuildEmbed("Logs are being updated, please try again in a few minutes."));
            return false;
        }

        return true;
    }

    private async Task HandleAutocompleteAsync(SocketAutocompleteInteraction interaction)
    {
        var cacheKey = $"Autocomplete_{interaction.Data.CommandName}_{interaction.Data.Current.Name}";
        var fileName = interaction.Data.Current.Name switch
        {
            "item" when interaction.Data.CommandName == CommandIds.FindLogs => "Items.json",
            "item" when interaction.Data.CommandName == CommandIds.LockBox => "LockboxItems.json",
            "slime" => "SlimeCodes.json",
            _ => throw new InvalidOperationException($"Unknown autocomplete option: {interaction.Data.Current.Name}")
        };

        if (!cache.TryGetValue(cacheKey, out List<AutocompleteResult>? suggestions) || suggestions is null)
        {
            var items = await jsonFileReader.ReadAsync<IReadOnlyList<string>>(Path.Combine("Data", "Autocomplete", fileName));
            suggestions = items.Select(x => new AutocompleteResult(x, x)).ToList();
            cache.Set(cacheKey, suggestions, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) });
        }

        var input = interaction.Data.Current.Value.ToString()?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
        await interaction.RespondAsync(suggestions.Where(s => input.All(word => s.Name.Contains(word, StringComparison.OrdinalIgnoreCase))).Take(25));
    }
}
