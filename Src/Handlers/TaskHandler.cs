using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Kozma.net.Src.Handlers;

public class TaskHandler(IBot bot,
    IConfiguration config,
    IBotLogger logger,
    IEmbedHandler embedHandler,
    IRoleHandler roleHandler,
    IUpdateHelper updateHelper,
    ITaskService taskService,
    IExchangeService exchangeService,
    IFileReader jsonFileReader) : ITaskHandler
{
    private sealed record TaskConfig(double Interval, Func<Task> ExecuteAsync);
    private sealed record Reminder(string Title, string Description);
    private sealed record Offer(int Price, int Volume);
    private sealed record EnergyMarketData(DateTime Datetime, int LastPrice, IReadOnlyCollection<Offer> BuyOffers, IReadOnlyCollection<Offer> SellOffers);

    private readonly DiscordSocketClient _client = bot.GetClient();
    private readonly Dictionary<string, TaskConfig> _tasks = new();
    private static readonly Random _random = new();
    private static readonly JsonSerializerOptions _marketOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task LaunchTasksAsync()
    {
        _tasks.Add("energyMarket", new TaskConfig(6, PostEnergyMarketAsync));
        _tasks.Add("slowmodeReminder", new TaskConfig(36, PostSlowModeReminderAsync));
        _tasks.Add("scamPrevention", new TaskConfig(72, PostScamPreventionAsync));
        _tasks.Add("newLogs", new TaskConfig(6, CheckForNewLogsAsync));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(CheckForExpiredTasksAsync);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        await Task.CompletedTask;
    }

    private async Task CheckForExpiredTasksAsync()
    {
        await UpdateActivityAsync();
        await roleHandler.CheckExpiredMutesAsync();

        var tasks = await taskService.GetTasksAsync(except: "offlineMutes");
        foreach (var task in tasks)
        {
            var taskConfig = _tasks[task.Name];
            if (task.UpdatedAt.AddHours(taskConfig.Interval) > DateTime.Now) continue;

            await taskConfig.ExecuteAsync();
            await taskService.UpdateTaskAsync(task.Name);
        }

        await Task.Delay(TimeSpan.FromMinutes(30));
        await PostStillConnectedAsync();
        await CheckForExpiredTasksAsync();
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
            6 => "The Devilite podcast",
            7 => "m.sound_test",
            8 => "Harry Mack",
            9 => "Traversing the Aurora Isles",
            10 => "Feedback",
            11 => "Chawkthree Weapon Demonstrations",
            12 => "Tier 4 Ice Queen With Commentary",
            13 => "Gun Guides: Ep. 10",
            14 => "Gatemap Viewer",
            15 => "m.jellycube_arena",
            16 => "Soup making tutorial",
            _ => "Avengers: Age of Ultron"
        };

        await bot.UpdateActivityAsync(name, type: random < 11 ? ActivityType.Listening : ActivityType.Watching);
    }

    private async Task PostEnergyMarketAsync()
    {
        try
        {
            if (await _client.GetChannelAsync(config.GetValue<ulong>("ids:marketChannel")) is not IMessageChannel channel) return;

            using var client = new HttpClient();
            var response = await client.GetAsync(new Uri(DotNetEnv.Env.GetString("energyMarket")));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<EnergyMarketData>(json, _marketOptions) ?? throw new ArgumentNullException();
            // TODO: log when not up to date?

            var rate = CalculateExchangeRate(data.BuyOffers, data.SellOffers);

            var fields = new List<EmbedFieldBuilder>
            {
                { embedHandler.CreateField($"Top Offers to Buy {Emotes.Energy} 100", data.BuyOffers.Aggregate("", (current, offer) => current + $"\n{Emotes.Crown} {offer.Price:N0} x {offer.Volume:N0}")) },
                { embedHandler.CreateEmptyField() },
                { embedHandler.CreateField($"Top Offers to Sell {Emotes.Energy} 100", data.SellOffers.Aggregate("", (current, offer) => current + $"\n{Emotes.Crown} {offer.Price:N0} x {offer.Volume:N0}")) },
            };

            var embed = embedHandler.GetBasicEmbed(data.Datetime.ToString("ddd, dd MMM yyyy"))
                .WithDescription($"**Last trade price: {Emotes.Crown} {data.LastPrice:N0}**\n**Recommended conversion rate: {Emotes.Crown} {rate} per {Emotes.Energy} 1**")
                .WithFields(fields);

            await exchangeService.UpdateExchangeAsync(rate);
            await channel.SendMessageAsync(embed: embed.Build());
            logger.Log(LogLevel.Moderation, "Posted latest Energy Market");
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex.Message);
            await logger.LogAsync($"error while fetching data from energy market api\n{ex.Message}", pingOwner: true);
        }
    }

    private static int CalculateExchangeRate(IReadOnlyCollection<Offer> buyOffers, IReadOnlyCollection<Offer> sellOffers)
    {
        var buyAverage = buyOffers.Sum(x => x.Price * x.Volume) / buyOffers.Sum(x => x.Volume);
        var sellAverage = sellOffers.Sum(x => x.Price * x.Volume) / sellOffers.Sum(x => x.Volume);
        return (buyAverage + (sellAverage - buyAverage) / 2) / 100;
    }

    private async Task PostSlowModeReminderAsync()
    {
        if (await _client.GetChannelAsync(config.GetValue<ulong>("ids:wtsChannel")) is not IMessageChannel wtsChannel) return;
        if (await _client.GetChannelAsync (config.GetValue<ulong>("ids:wtbChannel")) is not IMessageChannel wtbChannel) return;

        var embed = embedHandler.GetBasicEmbed($"This message is a reminder of the __{config.GetValue<int>("timers:slowmodeHours")} hour slowmode__ in this channel.")
            .WithDescription("You can edit your posts through the **/tradepostedit** command.\nWe apologise for any inconvenience this may cause.")
            .WithFields(new List<EmbedFieldBuilder>() { embedHandler.CreateField(Emotes.Empty, "Interested in what an item has sold for in the past?\nUse the **/findlogs** command.") })
            .Build();

        await wtsChannel.SendMessageAsync(embed: embed);
        await wtbChannel.SendMessageAsync(embed: embed);
        logger.Log(LogLevel.Moderation, "Posted slowmode reminders");
    }

    private async Task PostScamPreventionAsync()
    {
        if (await _client.GetChannelAsync(config.GetValue<ulong>("ids:wtsChannel")) is not IMessageChannel channel) return;
        var reminders = await jsonFileReader.ReadAsync<IReadOnlyList<Reminder>>(Path.Combine("Data", "Reminders.json"));
        var reminder = reminders[_random.Next(reminders.Count)];

        var embed = embedHandler.GetBasicEmbed("Trading Guidelines")
            .WithFields(new List<EmbedFieldBuilder>() { embedHandler.CreateField(reminder.Title, reminder.Description) })
            .WithFooter(new EmbedFooterBuilder()
                    .WithText("Information and communication is essential to negotiations. Please be careful.")
                    .WithIconUrl(_client.CurrentUser.GetDisplayAvatarUrl()));

        await channel.SendMessageAsync(embed: embed.Build());
        logger.Log(LogLevel.Moderation, "Posted scam prevention reminder");
    }

    private async Task CheckForNewLogsAsync()
    {
        var message = "Checking for new tradelogs";
        logger.Log(LogLevel.Moderation, message);
        await logger.LogAsync(embed: logger.GetLogEmbed(message, Colors.Moderation).Build());

        var channels = updateHelper.GetChannels();
        foreach (var channelData in channels)
        {
            if (await _client.GetChannelAsync(channelData.Value) is not IMessageChannel channel) return;

            if (channel is IThreadChannel thread)
            {
                await thread.ModifyAsync(t => t.Archived = true);
                await thread.ModifyAsync(t => t.Archived = false);
            }

            var logs = await updateHelper.GetLogsAsync(channel);
            if (logs.Count > 0) await updateHelper.UpdateLogsAsync(logs);
        }
    }

    private async Task PostStillConnectedAsync() =>
        await logger.LogAsync(embed: logger.GetLogEmbed($"Connected since <t:{bot.GetReadyTimestamp()}:f> with {_client.Latency}ms latency.", Colors.Moderation).Build());
}
