using Discord;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Models;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Kozma.net.Src.Trackers;

public class StatPageTracker(IBot bot,
    IConfiguration config,
    IMemoryCache cache,
    IEmbedHandler embedHandler,
    ICostCalculator costCalculator,
    ICommandService commandService,
    IUserService userService,
    IUnboxService unboxService,
    IPunchService punchService,
    ITradeLogService tradeLogService) : IStatPageTracker
{
    private bool _buildingInProgess;
    private static readonly MemoryCacheEntryOptions _cacheOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };
    private const string _cacheKey = "stat_pages";

    public async Task BuildPagesAsync()
    {
        if (cache.TryGetValue(_cacheKey, out List<Embed>? _)) return;

        var timer = System.Diagnostics.Stopwatch.StartNew();
        var finalPages = new List<Embed>();
        var pages = new List<EmbedBuilder>();
        _buildingInProgess = true;

        var userCountTask = GetUserCountAsync();
        var commandUsageTask = commandService.GetCommandUsageAsync(isGame: false);
        var gameUsageTask = commandService.GetCommandUsageAsync(isGame: true);
        var cmdUserCountTask = userService.GetTotalUsersCountAsync();
        var unboxedCountTask = unboxService.GetBoxOpenedCountAsync();
        var logCountTask = tradeLogService.GetTotalLogCountAsync();
        var totalSearchedTask = tradeLogService.GetTotalSearchCountAsync();

        await Task.WhenAll(userCountTask, commandUsageTask, gameUsageTask, cmdUserCountTask, unboxedCountTask, logCountTask, totalSearchedTask);
        var kozmaGuild = bot.Client.GetGuild(config.GetValue<ulong>("ids:server"));

        pages.AddRange(BuildServerPages(userCountTask.Result));
        pages.Add(BuildStatEmbed("Command usage", "Command", "Used", await commandService.GetCommandsAsync(isGame: false, commandUsageTask.Result), commandUsageTask.Result));
        pages.Add(BuildStatEmbed("Games played", "Game", "Played", await commandService.GetCommandsAsync(isGame: true, commandUsageTask.Result), commandUsageTask.Result));
        pages.Add(await BuildUserPageAsync(commandUsageTask.Result, forUnboxed: false, cmdUserCountTask.Result));
        pages.Add(await BuildUnboxPageAsync(unboxedCountTask.Result));
        pages.Add(await BuildUserPageAsync(unboxedCountTask.Result, forUnboxed: true));
        pages.Add(await BuildGamblerPageAsync(gameUsageTask.Result - unboxedCountTask.Result));
        pages.Add(BuildStatEmbed("All loggers", "User", "Posts", await tradeLogService.GetLogStatsAsync(authors: true, logCountTask.Result), logCountTask.Result));
        pages.Add(BuildStatEmbed("Tradelog channels", "Channel", "Posts", await tradeLogService.GetLogStatsAsync(authors: false, logCountTask.Result), logCountTask.Result));
        pages.Add(await BuildItemCountPageAsync(authors: true));
        pages.Add(await BuildItemCountPageAsync(authors: false));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "equipment"], [
            "Overcharged Mixmaster", "Celestial Orbitgun", "Somnambulists Totem", "Daybreaker Band", "Asi Very High Ctr Very High", "Black Kat Cowl", "Black Kat Raiment", "Black Kat Cloak",
            "Brandish", "Combuster", "Voltedge", "Autogun", "Blitz Needle", "Electron Vortex", "Spiral Soaker", "Caladbolg"]));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "equipment"], ["Celestial Shield", "Celestial Saber", "Celestial Vortex", "Celestial Orbitgun"]));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "equipment"], ["ctr very high", "ctr high", "asi very high", "asi high", "fire max", "normal max"]));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "miscellaneous"], ["Iron Lockbox", "Mirrored Lockbox", "Titanium Lockbox", "Silver Lockbox", "Steel Lockbox", "Platinum Lockbox", "Copper Lockbox", "Gold Lockbox", "Slime Lockbox"]));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "miscellaneous"], ["Gun Pup", "Spiraltail", "Piggy", "Spiralhorn", "Snarblepup", "Love Puppy"]));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "miscellaneous"], ["Extra Short Height Modifier", "Extra Tall Height Modifier", "Book of Dark Rituals", "Silver Personal Color"]));
        pages.Add(await BuildTermOccurencePageAsync([], ["Cool", "Dusky", "Fancy", "Heavy", "Military", "Regal", "Toasty"]));
        pages.Add(await BuildFindLogsPageAsync(totalSearchedTask.Result));
        timer.Stop();

        var fields = new List<EmbedFieldBuilder>()
        {
            embedHandler.CreateField("Bot Id", bot.Client.CurrentUser.Id.ToString()),
            embedHandler.CreateField("Bot Name", bot.Client.CurrentUser.Username),
            embedHandler.CreateField("Execution Time", $"{timer.Elapsed.TotalSeconds:F2}s"),
            embedHandler.CreateField("Running Since", $"<t:{bot.ReadyTimeStamp}:f>"),
            embedHandler.CreateField("Round-trip Latency", $"{bot.Client.Latency}ms"),
            embedHandler.CreateField("Commands Used", $"{commandUsageTask.Result:N0}"),
            embedHandler.CreateField("Unique Bot Users", $"{cmdUserCountTask.Result:N0}"),
            embedHandler.CreateField("Servers", $"{bot.Client.Guilds.Count:N0}"),
            embedHandler.CreateField("Unique Users", $"{userCountTask.Result:N0}"),
            embedHandler.CreateField("Tradelogs", $"{logCountTask.Result:N0}"),
            embedHandler.CreateField("Items Searched", $"{totalSearchedTask.Result:N0}"),
            embedHandler.CreateField("Server Members", $"{kozmaGuild!.MemberCount:N0}"),
        };

        pages.Insert(0, embedHandler.GetBasicEmbed("General info").WithFields(fields));

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].Title = $"{pages[i].Title} - {i + 1}/{pages.Count}";
            pages[i].Footer = new EmbedFooterBuilder()
                .WithText($"{i + 1}/{pages.Count}")
                .WithIconUrl(bot.Client.CurrentUser.GetDisplayAvatarUrl());

            finalPages.Add(pages[i].Build());
        }

        cache.Set(_cacheKey, finalPages, _cacheOptions);
        _buildingInProgess = false;
    }

    public Embed GetPage(ulong id, string action = "")
    {
        if (_buildingInProgess) return embedHandler.GetAndBuildEmbed("Pages are being built, please try again later.");
        if (!cache.TryGetValue(_cacheKey, out List<Embed>? pages) || pages is null) return embedHandler.GetAndBuildEmbed("Pages don't exist anymore, rerun /stats");

        if (!cache.TryGetValue(id, out int currentPage))
        {
            currentPage = 0;
        }

        switch (action)
        {
            case ComponentIds.StatsFirst: currentPage = 0; break;
            case ComponentIds.StatsPrev when currentPage > 0: currentPage--; break;
            case ComponentIds.StatsNext when currentPage < pages.Count - 1: currentPage++; break;
            case ComponentIds.StatsLast: currentPage = pages.Count - 1; break;
            default: currentPage = 0; break;
        }

        cache.Set(id, currentPage, _cacheOptions);
        return pages[currentPage];
    }

    public MessageComponent GetComponents(ulong id)
    {
        if (!cache.TryGetValue(id, out int page)) page = 0;
        if (!cache.TryGetValue(_cacheKey, out List<Embed>? pages) || pages is null) pages = [];

        return new ComponentBuilder()
            .WithButton(label: Emotes.First, customId: ComponentIds.StatsBase + ComponentIds.StatsFirst, style: ButtonStyle.Primary, disabled: page == 0)
            .WithButton(label: Emotes.Previous, customId: ComponentIds.StatsBase + ComponentIds.StatsPrev, style: ButtonStyle.Primary, disabled: page == 0)
            .WithButton(label: Emotes.Next, customId: ComponentIds.StatsBase + ComponentIds.StatsNext, style: ButtonStyle.Primary, disabled: page >= pages.Count - 1)
            .WithButton(label: Emotes.Last, customId: ComponentIds.StatsBase + ComponentIds.StatsLast, style: ButtonStyle.Primary, disabled: page >= pages.Count - 1)
            .Build();
    }

    private async Task<int> GetUserCountAsync()
    {
        foreach (var guild in bot.Client.Guilds)
        {
            if (!guild.HasAllMembers) await guild.DownloadUsersAsync();
        }

        return bot.Client.Guilds
            .SelectMany(guild => guild.Users)
            .Where(member => !member.IsBot)
            .Select(member => member.Id)
            .Distinct()
            .Count();
    }

    private List<EmbedBuilder> BuildServerPages(int userCount)
    {
        var pages = new List<EmbedBuilder>();
        var serverPages = bot.Client.Guilds
            .OrderByDescending(g => g.MemberCount)
            .Select((server, index) => $"{index + 1}. {Format.Bold(server.Name)}: {server.Users.Count}")
            .Select((info, index) => new { info, index })
            .GroupBy(x => x.index / 20)
            .Select(group => string.Join("\n", group.Select(x => x.info)))
            .ToList();

        int totalEmbeds = (serverPages.Count + 1) / 2;
        for (int i = 0; i < serverPages.Count; i += 2)
        {
            var embed = embedHandler.GetBasicEmbed($"Servers ({i / 2 + 1}/{totalEmbeds})");
            var fields = new List<EmbedFieldBuilder>
            {
                embedHandler.CreateField(Emotes.Empty, serverPages[i]),
                embedHandler.CreateField(Emotes.Empty, i + 1 < serverPages.Count ? serverPages[i + 1] : Emotes.Empty),
                embedHandler.CreateEmptyField(),
                embedHandler.CreateField("Total", $"{bot.Client.Guilds.Count:N0}"),
                embedHandler.CreateField("Unique Users", $"{userCount:N0}")
            };

            pages.Add(embed.WithFields(fields));
        }

        return pages;
    }

    private async Task<EmbedBuilder> BuildUserPageAsync(int totalUsed, bool forUnboxed, int totalUsers = 0)
    {
        var limit = 20;
        var data = await userService.GetUsersAsync(limit, totalUsed, forUnboxed);
        var (names, counts, percentages) = GetBasicFieldValues(data);

        var fields = new List<EmbedFieldBuilder>()
        {
            embedHandler.CreateField("User", names),
            embedHandler.CreateField(forUnboxed ? "Opened" : "Commands", counts),
            embedHandler.CreateField("Percentage", percentages)
        };
        if (totalUsers > 0) embedHandler.CreateField("Unique Users", $"{totalUsers:N0}");
        fields.Add(embedHandler.CreateField("Total", $"{totalUsed:N0}"));

        return embedHandler.GetBasicEmbed($"Top {limit} {(forUnboxed ? "unboxers" : "bot users")}").WithFields(fields);
    }

    private async Task<EmbedBuilder> BuildUnboxPageAsync(int total)
    {
        var data = await unboxService.GetBoxesAsync(total);
        var boxes = new StringBuilder();
        var opened = new StringBuilder();
        var percentages = new StringBuilder();
        var energy = 0.0;
        var dollars = 0.0;

        var index = 1;
        foreach (var box in data)
        {
            var boxData = box.Name.ToBoxData();

            switch (boxData.Currency)
            {
                case BoxCurrency.Energy: energy += boxData.Price; break;
                case BoxCurrency.Dollar: dollars += costCalculator.CalculateBoxCost(box.Count, boxData); break;
            }

            boxes.AppendLine($"{index} {Format.Bold(box.Name.ToString())}");
            opened.AppendLine($"{box.Count:N0}");
            percentages.AppendLine($"{box.Percentage:P2}");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedHandler.CreateField("Box", boxes.ToString()),
            embedHandler.CreateField("Opened", opened.ToString()),
            embedHandler.CreateField("Percentage", percentages.ToString()),
            embedHandler.CreateField("Total", $"{total:N0}"),
            embedHandler.CreateField("Energy", $"{energy:N0}"),
            embedHandler.CreateField("Dollars", $"${dollars:N2}"),
        };

        return embedHandler.GetBasicEmbed("Unbox command").WithFields(fields);
    }

    private async Task<EmbedBuilder> BuildGamblerPageAsync(int sessions)
    {
        var limit = 20;
        var totalSpent = await punchService.GetTotalSpentAsync();
        var data = await punchService.GetGamblersAsync(limit, totalSpent);
        var (names, counts, percentages) = GetBasicFieldValues(data);

        var fields = new List<EmbedFieldBuilder>()
        {
            embedHandler.CreateField("User", names),
            embedHandler.CreateField("Crowns Spent", counts),
            embedHandler.CreateField("Percentage", percentages),
            embedHandler.CreateField("Total", $"{totalSpent:N0}"),
            embedHandler.CreateField("Sessions", $"{sessions:N0}")
        };

        return embedHandler.GetBasicEmbed($"Top {limit} highest spenders at Punch").WithFields(fields);
    }

    private async Task<EmbedBuilder> BuildFindLogsPageAsync(int totalSearched)
    {
        var limit = 20;
        var data = await tradeLogService.GetSearchedLogsAsync(limit);
        var names = new StringBuilder();
        var searches = new StringBuilder();

        var index = 1;
        foreach (var item in data)
        {
            names.AppendLine($"{index} {Format.Bold(item.Item.ToTitleCase())}");
            searches.AppendLine($"{item.Count:N0}");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedHandler.CreateField("Item", names.ToString()),
            embedHandler.CreateField("Searches", searches.ToString()),
            embedHandler.CreateField("Unique Searches", $"{totalSearched:N0}", isInline: false),
        };

        return embedHandler.GetBasicEmbed($"Top {limit} searched items").WithFields(fields);
    }

    private async Task<EmbedBuilder> BuildItemCountPageAsync(bool authors)
    {
        var (data, total) = await tradeLogService.GetItemCountAsync(authors);

        return BuildStatEmbed("Estimated logged items", authors ? "User" : "Channel", "Items", data, total);
    }

    private async Task<EmbedBuilder> BuildTermOccurencePageAsync(List<string> channels, List<string> terms)
    {
        var (data, total) = await tradeLogService.CountOccurencesAsync(channels, terms);
        var names = new StringBuilder();
        var searches = new StringBuilder();

        var index = 1;
        foreach (var term in data)
        {
            names.AppendLine($"{index} {Format.Bold(term.Key)}");
            searches.AppendLine($"{term.Value:N0}");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedHandler.CreateField("Logged", names.ToString()),
            embedHandler.CreateField("Count", searches.ToString()),
            embedHandler.CreateField("Total", $"{total:N0}", isInline: false),
        };

        return embedHandler.GetBasicEmbed("Estimated logged items").WithFields(fields);
    }

    private EmbedBuilder BuildStatEmbed(string title, string field1, string field2, IEnumerable<DbStat> data, int total)
    {
        var (names, counts, percentages) = GetBasicFieldValues(data);
        var fields = new List<EmbedFieldBuilder>()
        {
            embedHandler.CreateField(field1, names),
            embedHandler.CreateField(field2, counts),
            embedHandler.CreateField("Percentage", percentages),
            embedHandler.CreateField("Total", $"{total:N0}"),
        };

        return embedHandler.GetBasicEmbed(title).WithFields(fields);
    }

    private static (string names, string counts, string percentages) GetBasicFieldValues(IEnumerable<DbStat> data)
    {
        var names = new StringBuilder();
        var counts = new StringBuilder();
        var percentages = new StringBuilder();

        var index = 1;
        foreach (var item in data)
        {
            names.AppendLine($"{index} {Format.Bold(item.Name)}");
            counts.AppendLine($"{item.Count:N0}");
            percentages.AppendLine($"{item.Percentage:P2}");
            index++;
        }

        return (names.ToString(), counts.ToString(), percentages.ToString());
    }
}
