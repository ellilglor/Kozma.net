﻿using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace Kozma.net.Src.Handlers;

public class TaskHandler(IBot bot, IConfiguration config, IBotLogger logger, IEmbedHandler embedHandler, ITaskService taskService, IFileReader jsonFileReader) : ITaskHandler
{
    private record TaskConfig(double Interval, Func<Task> ExecuteAsync);
    private record Reminder(string Title, string Description);

    private readonly DiscordSocketClient _client = bot.GetClient();

    private readonly Dictionary<string, TaskConfig> _tasks = new();

    private static readonly Random _random = new();

    public void Initialize()
    {
        _tasks.Add("energyMarket", new TaskConfig(12, async () => await PostEnergyMarketAsync()));
        _tasks.Add("slowmodeReminder", new TaskConfig(36, async () => await PostSlowModeReminderAsync()));
        _tasks.Add("scamPrevention", new TaskConfig(72, async () => await PostScamPreventionAsync()));
        _tasks.Add("newLogs", new TaskConfig(6, async () => await CheckForNewLogsAsync()));

        _client.Ready += OnReadyAsync;
    }

    private async Task OnReadyAsync()
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () => await CheckForExpiredTasksAsync());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        await Task.CompletedTask;
    }

    private async Task CheckForExpiredTasksAsync()
    {
        // TODO check expired mutes
        var tasks = await taskService.GetTasksAsync(except: "offlineMutes");
        var currentTime = DateTime.Now;

        foreach (var task in tasks)
        {
            var taskConfig = _tasks[task.Name];
            //if (task.UpdatedAt.AddHours(taskConfig.Interval) > currentTime) continue;

            await taskConfig.ExecuteAsync();
            //await taskService.UpdateTaskAsync(task.Name);
        }

        await Task.Delay(TimeSpan.FromMinutes(1));
        await CheckForExpiredTasksAsync();
    }

    private async Task PostEnergyMarketAsync()
    {
        logger.Log(Enums.LogColor.Moderation, "energy");
        await Task.CompletedTask;
    }

    private async Task PostSlowModeReminderAsync()
    {
        if (_client.GetChannel(config.GetValue<ulong>("ids:wtsChannel")) is not SocketTextChannel wtsChannel) return;
        if (_client.GetChannel(config.GetValue<ulong>("ids:wtbChannel")) is not SocketTextChannel wtbChannel) return;

        var embed = embedHandler.GetBasicEmbed($"This message is a reminder of the __{config.GetValue<int>("timers:slowmodeHours")} hour slowmode__ in this channel.")
            .WithDescription("You can edit your posts through the **/tradepostedit** command.\nWe apologise for any inconvenience this may cause.")
            .WithFields(new List<EmbedFieldBuilder>() { embedHandler.CreateField("\u200B", "Interested in what an item has sold for in the past?\nUse the **/findlogs** command.") });

        /*await wtsChannel.SendMessageAsync(embed: embed.Build());
        await wtbChannel.SendMessageAsync(embed: embed.Build());*/
    }

    private async Task PostScamPreventionAsync()
    {
        if (_client.GetChannel(config.GetValue<ulong>("ids:wtsChannel")) is not SocketTextChannel channel) return;
        var reminders = await jsonFileReader.ReadAsync<List<Reminder>>(Path.Combine("Data", "Reminders.json"));
        if (reminders is null) return;

        var reminder = reminders[_random.Next(reminders.Count)];
        var embed = embedHandler.GetBasicEmbed("Trading Guidelines")
            .WithFields(new List<EmbedFieldBuilder>() { embedHandler.CreateField(reminder.Title, reminder.Description) })
            .WithFooter(new EmbedFooterBuilder()
                    .WithText("Information and communication is essential to negotiations. Please be careful.")
                    .WithIconUrl(_client.CurrentUser.GetDisplayAvatarUrl()));

        //await channel.SendMessageAsync(embed: embed.Build());
    }

    private async Task CheckForNewLogsAsync()
    {
        logger.Log(Enums.LogColor.Moderation, "logs");
        await Task.CompletedTask;
    }
}
