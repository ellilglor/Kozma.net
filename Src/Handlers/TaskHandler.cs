using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Models.Entities;
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
    IUserService userService,
    IExchangeService exchangeService,
    IFileReader jsonFileReader) : ITaskHandler
{
    private record TaskConfig(double Interval, Func<Task> ExecuteAsync);
    private record Reminder(string Title, string Description);
    private record Offer(int Price, int Volume);
    private record EnergyMarketData(DateTime Datetime, int LastPrice, List<Offer> BuyOffers, List<Offer> SellOffers);

    private readonly DiscordSocketClient _client = bot.GetClient();
    private readonly Dictionary<string, TaskConfig> _tasks = new();
    private static readonly Random _random = new();
    private static readonly JsonSerializerOptions _marketOptions = new() { PropertyNameCaseInsensitive = true };

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
        await CheckExpiredMutesAsync();

        var tasks = await taskService.GetTasksAsync(except: "offlineMutes");
        foreach (var task in tasks)
        {
            var taskConfig = _tasks[task.Name];
            if (task.UpdatedAt.AddHours(taskConfig.Interval) > DateTime.Now) continue;

            await taskConfig.ExecuteAsync();
            await taskService.UpdateTaskAsync(task.Name);
        }

        await Task.Delay(TimeSpan.FromMinutes(5));
        await PostStillConnectedAsync();
        await CheckForExpiredTasksAsync();
    }

    private async Task CheckExpiredMutesAsync()
    {
        logger.Log(LogColor.Moderation, "Checking expired mutes");
        var sellMutes = await userService.GetAndDeleteExpiredMutesAsync<SellMute>();
        var buyMutes = await userService.GetAndDeleteExpiredMutesAsync<BuyMute>();

        if (!sellMutes.Any() && !buyMutes.Any()) return; // Both are empty
        var guild = _client.Guilds.FirstOrDefault(g => g.Id.Equals(config.GetValue<ulong>("ids:server")))!;
        if (!guild.HasAllMembers) await guild.DownloadUsersAsync(); // Assure the users will be in the cache

        await RemoveExpiredMutesAsync(guild, config.GetValue<ulong>("ids:wtsRole"), sellMutes);
        await RemoveExpiredMutesAsync(guild, config.GetValue<ulong>("ids:wtbRole"), buyMutes);
    }

    private async Task RemoveExpiredMutesAsync<T>(SocketGuild guild, ulong roleId, IEnumerable<T> mutes) where T : Mute
    {
        foreach (var m in mutes)
        {
            if (guild.GetUser(ulong.Parse(m.Id)) is not SocketGuildUser user) continue; // User left the server
            await roleHandler.RemoveRoleAsync(user, roleId);
        }
    }

    private async Task PostEnergyMarketAsync()
    {
        try
        {
            if (_client.GetChannel(config.GetValue<ulong>("ids:marketChannel")) is not SocketTextChannel channel) return;

            using var client = new HttpClient();
            var response = await client.GetAsync(new Uri(config.GetValue<string>("energyMarket")!));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<EnergyMarketData>(json, _marketOptions) ?? throw new Exception();

            var crown = "<:kbpcrowns:1092398578053431366>";
            var energy = "<:kbpenergy:1092398618939506718>";
            var buyAverage = data.BuyOffers.Sum(x => x.Price * x.Volume) / data.BuyOffers.Sum(x => x.Volume);
            var sellAverage = data.SellOffers.Sum(x => x.Price * x.Volume) / data.SellOffers.Sum(x => x.Volume);
            var rate = (buyAverage + (sellAverage - buyAverage) / 2) / 100;

            var fields = new List<EmbedFieldBuilder>
            {
                { embedHandler.CreateField($"Top Offers to Buy {energy} 100", data.BuyOffers.Aggregate("", (current, offer) => current + $"\n{crown} {offer.Price:N0} x {offer.Volume:N0}")) },
                { embedHandler.CreateEmptyField() },
                { embedHandler.CreateField($"Top Offers to Sell {energy} 100", data.SellOffers.Aggregate("", (current, offer) => current + $"\n{crown} {offer.Price:N0} x {offer.Volume:N0}")) },
            };

            var embed = embedHandler.GetBasicEmbed(data.Datetime.ToString("ddd, dd MMM yyyy"))
                .WithDescription($"**Last trade price: {crown} {data.LastPrice:N0}**\n**Recommended conversion rate: {crown} {rate} per {energy} 1**")
                .WithFields(fields);

            await exchangeService.UpdateExchangeAsync(rate);
            await channel.SendMessageAsync(embed: embed.Build());
            logger.Log(LogColor.Moderation, "Posted latest Energy Market");
        }
        catch (Exception ex)
        {
            logger.Log(LogColor.Error, ex.Message);
            await logger.LogAsync($"<@{config.GetValue<ulong>("ids:owner")}> error while fetching data from energy market api\n{ex.Message}");
        }
    }

    private async Task PostSlowModeReminderAsync()
    {
        if (_client.GetChannel(config.GetValue<ulong>("ids:wtsChannel")) is not SocketTextChannel wtsChannel) return;
        if (_client.GetChannel(config.GetValue<ulong>("ids:wtbChannel")) is not SocketTextChannel wtbChannel) return;

        var embed = embedHandler.GetBasicEmbed($"This message is a reminder of the __{config.GetValue<int>("timers:slowmodeHours")} hour slowmode__ in this channel.")
            .WithDescription("You can edit your posts through the **/tradepostedit** command.\nWe apologise for any inconvenience this may cause.")
            .WithFields(new List<EmbedFieldBuilder>() { embedHandler.CreateField("\u200B", "Interested in what an item has sold for in the past?\nUse the **/findlogs** command.") })
            .Build();

        await wtsChannel.SendMessageAsync(embed: embed);
        await wtbChannel.SendMessageAsync(embed: embed);
        logger.Log(LogColor.Moderation, "Posted slowmode reminders");
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

        await channel.SendMessageAsync(embed: embed.Build());
        logger.Log(LogColor.Moderation, "Posted scam prevention reminder");
    }

    private async Task CheckForNewLogsAsync()
    {
        var message = "Checking for new tradelogs";
        logger.Log(LogColor.Moderation, message);
        await logger.LogAsync(embed: logger.GetLogEmbed(message, EmbedColor.Moderation).Build());

        var channels = updateHelper.GetChannels();
        foreach (var channelData in channels)
        {
            if (_client.GetChannel(channelData.Value) is not SocketTextChannel channel) return;

            if (channel is SocketThreadChannel thread)
            {
                await thread.ModifyAsync(t => t.Archived = true);
                await thread.ModifyAsync(t => t.Archived = false);
            }

            await updateHelper.UpdateLogsAsync(channel);
        }
    }

    private async Task PostStillConnectedAsync()
    {
        var embed = logger.GetLogEmbed($"Connected since <t:{bot.GetReadyTimestamp()}:f> with {_client.Latency}ms latency.", EmbedColor.Moderation);

        await logger.LogAsync(embed: embed.Build());
    }
}
