using Discord;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Models.Entities;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Handlers;

public class TaskHandler(IBot bot,
    IConfiguration config,
    IBotLogger logger,
    IEmbedHandler embedHandler,
    IRoleHandler roleHandler,
    IUpdateHelper updateHelper,
    ITaskService taskService,
    ITradeLogService tradeLogService,
    IExchangeService exchangeService,
    IFileReader jsonFileReader,
    IApiFetcher apiFetcher) : ITaskHandler
{
    private sealed record TaskConfig(double Interval, Func<Task<bool>> ExecuteAsync);
    private sealed record Reminder(string Title, string Description);
    private sealed record Offer(int Price, int Volume);
    private sealed record EnergyMarketData(DateTime Datetime, int LastPrice, IReadOnlyCollection<Offer> BuyOffers, IReadOnlyCollection<Offer> SellOffers);

    private static readonly Dictionary<string, TaskConfig> _tasks = new();
    private static DateTime _lastExecuted = DateTime.UtcNow;
    private static readonly Random _random = new();
    private static bool _hasBeenWarnedForApi = false;

    public async Task LaunchTasksAsync()
    {
        _lastExecuted = DateTime.UtcNow;

        _tasks.Clear();
        _tasks.Add("energyMarket", new TaskConfig(6, PostEnergyMarketAsync));
        _tasks.Add("slowmodeReminder", new TaskConfig(36, PostSlowModeReminderAsync));
        _tasks.Add("scamPrevention", new TaskConfig(72, PostScamPreventionAsync));
        _tasks.Add("onlineAHReminder", new TaskConfig(84, PostAuctionHouseReminderAsync));
        _tasks.Add("cleanBotLogs", new TaskConfig(48, ClearBotLogsAsync));
        _tasks.Add("resetLogs", new TaskConfig(168, ResetLogsAsync));
        _tasks.Add("newLogs", new TaskConfig(6, CheckForNewLogsAsync));
        _tasks.Add("outdatedMutes", new TaskConfig(36, roleHandler.CheckOutdatedMutesAsync));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(CheckForExpiredTasksAsync); // Run like this to not block the thread
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        await Task.CompletedTask;
    }

    public async Task CheckIfTaskHandlerIsRunningAsync()
    {
        if (_lastExecuted > DateTime.UtcNow.AddHours(-2)) return;

        logger.Log(LogLevel.Error, "Relaunching Tasks");
        await LaunchTasksAsync();
    }

    private async Task CheckForExpiredTasksAsync()
    {
        while (true)
        {
            await UpdateActivityAsync();
            await roleHandler.CheckExpiredMutesAsync();

            var tasks = await taskService.GetTasksAsync(except: "offlineMutes");
            foreach (var task in tasks)
            {
                var taskConfig = _tasks[task.Name];
                if (task.UpdatedAt.AddHours(taskConfig.Interval) > DateTime.Now) continue;

                try
                {
                    var success = await taskConfig.ExecuteAsync();
                    if (success) await taskService.UpdateTaskAsync(task.Name);
                }
                catch (Exception ex)
                {
                    await logger.LogAsync($"Error while executing task {task.Name}\n{ex.Message}", pingOwner: true);
                }
            }

            _lastExecuted = DateTime.UtcNow;

            await Task.Delay(TimeSpan.FromMinutes(30));
            await PostStillConnectedAsync();
        }
    }

    private async Task UpdateActivityAsync()
    {
        var random = _random.Next(0, 18);
        var name = random switch
        {
            0 => "/help",
            1 => "/findlogs",
            2 => "/unbox",
            3 => "/punch",
            4 => "Emberlight Radio",
            5 => "The Devilite podcast",
            6 => "m.sound_test",
            7 => "Harry Mack",
            8 => "Traversing the Aurora Isles",
            9 => "Feedback",
            10 => "Chawkthree Weapon Demonstrations",
            11 => "Tier 4 Ice Queen With Commentary",
            12 => "Gun Guides: Ep. 10",
            13 => "Gatemap Viewer",
            14 => "m.jellycube_arena",
            15 => "Soup making tutorial",
            _ => "Avengers: Age of Ultron"
        };

        await bot.UpdateActivityAsync(name, type: random < 10 ? ActivityType.Listening : ActivityType.Watching);
    }

    private async Task<bool> PostEnergyMarketAsync()
    {
        try
        {
            if (await bot.Client.GetChannelAsync(config.GetValue<ulong>("ids:channels:market")) is not IMessageChannel channel) return false;

            var data = await apiFetcher.FetchAsync<EnergyMarketData>(DotNetEnv.Env.GetString("energyMarket"), new() { PropertyNameCaseInsensitive = true });

            if (data.Datetime < DateTime.Now.AddDays(-1))
            {
                if (_hasBeenWarnedForApi) return false;
                if (await bot.Client.GetChannelAsync(config.GetValue<ulong>("ids:channels:dev")) is not IMessageChannel testChannel) return false;

                await testChannel.SendMessageAsync($"{MentionUtils.MentionUser(config.GetValue<ulong>("ids:ape"))}\nThe Energy Market api seems to be outdated, last updated: {data.Datetime}");
                _hasBeenWarnedForApi = true;
                return false;
            }

            var rate = CalculateExchangeRate(data.BuyOffers, data.SellOffers);

            var fields = new List<EmbedFieldBuilder>
            {
                { embedHandler.CreateField($"Top Offers to Buy {Emotes.Energy} 100", ExtractListings(data.BuyOffers)) },
                { embedHandler.CreateEmptyField() },
                { embedHandler.CreateField($"Top Offers to Sell {Emotes.Energy} 100", ExtractListings(data.SellOffers)) },
            };

            var embed = embedHandler.GetBasicEmbed(data.Datetime.ToString("ddd, dd MMM yyyy"))
                .WithDescription($"{Format.Bold($"Last trade price: {Emotes.Crown} {data.LastPrice:N0}")}\n{Format.Bold($"Recommended conversion rate: {Emotes.Crown} {rate} per {Emotes.Energy} 1")}")
                .WithFields(fields);

            await exchangeService.UpdateExchangeAsync(rate);
            await channel.SendMessageAsync(embed: embed.Build());
            logger.Log(LogLevel.Moderation, "Posted latest Energy Market");

            if (_hasBeenWarnedForApi) _hasBeenWarnedForApi = false; // Energy Market is running again => reset for next downtime
            return true;
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex.Message);
            await logger.LogAsync($"error while fetching data from energy market api\n{ex.Message}", pingOwner: true);
            return false;
        }
    }

    private static int CalculateExchangeRate(IReadOnlyCollection<Offer> buyOffers, IReadOnlyCollection<Offer> sellOffers)
    {
        var buyAverage = buyOffers.Sum(x => x.Price * x.Volume) / buyOffers.Sum(x => x.Volume);
        var sellAverage = sellOffers.Sum(x => x.Price * x.Volume) / sellOffers.Sum(x => x.Volume);
        return (buyAverage + (sellAverage - buyAverage) / 2) / 100;
    }

    private static string ExtractListings(IReadOnlyCollection<Offer> offers) =>
        offers.Aggregate("", (current, offer) => current + $"\n{Emotes.Crown} {offer.Price:N0} x {offer.Volume:N0}");

    private async Task<bool> PostSlowModeReminderAsync()
    {
        if (await bot.Client.GetChannelAsync(config.GetValue<ulong>("ids:channels:wts")) is not IMessageChannel wtsChannel) return false;
        if (await bot.Client.GetChannelAsync(config.GetValue<ulong>("ids:channels:wtb")) is not IMessageChannel wtbChannel) return false;

        var embed = embedHandler.GetBasicEmbed($"This message is a reminder of the {Format.Underline($"{config.GetValue<int>("timers:slowmodeHours")} hour slowmode")} in this channel.")
            .WithDescription($"You can edit your posts through the {Format.Code("/tradepostedit")} command.\nYou can use this command in any channel within this server.\nWe apologize for any inconvenience this may cause.")
            .WithFields(new List<EmbedFieldBuilder>() { embedHandler.CreateField(Emotes.Empty, $"Interested in what an item has sold for in the past?\nUse the {Format.Code("/findlogs")} command.") })
            .Build();

        await wtsChannel.SendMessageAsync(embed: embed);
        await wtbChannel.SendMessageAsync(embed: embed);
        logger.Log(LogLevel.Moderation, "Posted slowmode reminders");
        return true;
    }

    private async Task<bool> PostScamPreventionAsync()
    {
        if (await bot.Client.GetChannelAsync(config.GetValue<ulong>("ids:channels:wts")) is not IMessageChannel channel) return false;
        var reminders = await jsonFileReader.ReadAsync<IReadOnlyList<Reminder>>(Path.Combine("Data", "Reminders.json"));
        var reminder = reminders[_random.Next(reminders.Count)];

        var embed = embedHandler.GetBasicEmbed("Trading Guidelines")
            .WithFields(new List<EmbedFieldBuilder>() { embedHandler.CreateField(reminder.Title, reminder.Description) })
            .WithFooter(new EmbedFooterBuilder()
                    .WithText("Information and communication is essential to negotiations. Please be careful.")
                    .WithIconUrl(bot.Client.CurrentUser.GetDisplayAvatarUrl()));

        await channel.SendMessageAsync(embed: embed.Build());
        logger.Log(LogLevel.Moderation, "Posted scam prevention reminder");
        return true;
    }

    private async Task<bool> PostAuctionHouseReminderAsync()
    {
        if (await bot.Client.GetChannelAsync(config.GetValue<ulong>("ids:channels:general")) is not IMessageChannel channel) return false;

        var embed = embedHandler.GetBasicEmbed("The online Auction House")
            .WithDescription($"Want to check out what is available on the auction house, or want to know when a listing is about to end? Then check out {Format.Bold(Format.Url("this project", config.GetValue<string>("auctionHouse")))}.")
            .WithFields(new List<EmbedFieldBuilder>() { embedHandler.CreateField("Features", "- Current & historic Auction House listings\n- Up-to-Date Energy Market\n- Full item catalog") })
            .WithFooter(new EmbedFooterBuilder().WithText($"Brought to you by {Emotes.Ape}"));

        await channel.SendMessageAsync(embed: embed.Build());
        logger.Log(LogLevel.Moderation, "Posted online Auction House reminder");
        return true;
    }

    private async Task<bool> CheckForNewLogsAsync()
    {
        var logs = new List<TradeLog>();
        var channels = updateHelper.GetChannels();
        var message = "Checking for new tradelogs";
        logger.Log(LogLevel.Moderation, message);
        await logger.LogAsync(embed: embedHandler.GetLogEmbed(message, Colors.Moderation).Build());

        foreach (var channelData in channels)
        {
            if (await bot.Client.GetChannelAsync(channelData.Value) is not IMessageChannel channel) continue;

            if (channel is IThreadChannel thread)
            {
                await thread.ModifyAsync(t => t.Archived = true);
                await thread.ModifyAsync(t => t.Archived = false);
            }

            logs.AddRange(await updateHelper.GetLogsAsync(channel));
        }

        if (logs.Count > 0)
        {
            updateHelper.ClearFindLogsCache();
            await tradeLogService.UpdateLogsAsync(logs);
        }

        return true;
    }

    private async Task<bool> ResetLogsAsync()
    {
        var logs = new List<TradeLog>();
        var channels = updateHelper.GetChannels();
        var message = "Resetting tradelogs";
        logger.Log(LogLevel.Moderation, message);
        await logger.LogAsync(embed: embedHandler.GetLogEmbed(message, Colors.Moderation).Build());

        var tasks = channels.Select(async (channelData) =>
        {
            if (await bot.Client.GetChannelAsync(channelData.Value) is not IMessageChannel channel) return;

            logs.AddRange(await updateHelper.GetLogsAsync(channel, limit: int.MaxValue));
        }).ToList();

        await Task.WhenAll(tasks);
        logger.Log(LogLevel.Moderation, "Uploading logs to database...");

        updateHelper.ClearFindLogsCache();
        await tradeLogService.DeleteAndUpdateLogsAsync(logs);

        logger.Log(LogLevel.Moderation, "Tradelogs have been reset");
        return true;
    }

    private async Task<bool> ClearBotLogsAsync()
    {
        if (await bot.Client.GetChannelAsync(config.GetValue<ulong>("ids:channels:kozmaLogs")) is not ITextChannel channel) return false;

        logger.Log(LogLevel.Moderation, "Cleaning BotLogs channel");
        var messages = await channel.GetMessagesAsync(limit: 420).FlattenAsync();
        var toDelete = new List<IMessage>();
        var messageAgeLimit = DateTimeOffset.UtcNow.AddDays(-14);

        foreach (var msg in messages)
        {
            if (msg.CreatedAt <= messageAgeLimit) break;
            if (msg.Embeds.Count == 0) continue;
            if (msg.Embeds.First().Color != Colors.Moderation) continue;

            toDelete.Add(msg);
            if (toDelete.Count == DiscordConfig.MaxMessagesPerBatch)
            {
                await channel.DeleteMessagesAsync(toDelete);
                toDelete.Clear();
            }
        }

        if (toDelete.Count > 0) await channel.DeleteMessagesAsync(toDelete);

        return true;
    }

    private async Task PostStillConnectedAsync() =>
        await logger.LogAsync(embed: embedHandler.GetLogEmbed($"Connected since <t:{bot.ReadyTimeStamp}:f> with {bot.Client.Latency}ms latency.", Colors.Moderation).Build());
}
