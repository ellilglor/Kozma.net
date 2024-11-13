using Discord;
using Discord.WebSocket;
using Kozma.net.Factories;
using Kozma.net.Helpers;
using Kozma.net.Models;
using Kozma.net.Services;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Kozma.net.Trackers;

public class StatPageTracker(IBot bot,
    IConfiguration config,
    IEmbedFactory embedFactory,
    IBoxHelper boxHelper,
    ICommandService commandService,
    IUserService userService,
    IUnboxService unboxService,
    IPunchService punchService,
    ITradeLogService tradeLogService) : IStatPageTracker
{
    private readonly DiscordSocketClient _client = bot.GetClient();
    private List<Embed> _pages = [];
    private readonly Dictionary<ulong, int> _users = [];
    private bool _buildingInProgess = false;

    public async Task BuildPagesAsync()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var pages = new List<EmbedBuilder>();
        _buildingInProgess = true;
        _pages = [];

        var userCount = await GetUserCountAsync();
        var commandUsage = await commandService.GetCommandUsageAsync(isGame: false);
        var gameUsage = await commandService.GetCommandUsageAsync(isGame: true);
        var cmdUserCount = await userService.GetTotalUsersCountAsync();
        var unboxedCount = await unboxService.GetBoxOpenedCountAsync();
        var logCount = await tradeLogService.GetTotalLogCountAsync();
        var totalSearched = await tradeLogService.GetTotalSearchCountAsync();
        var kozmaGuild = _client.Guilds.FirstOrDefault(g => g.Id.Equals(config.GetValue<ulong>("ids:serverId")));

        pages.AddRange(BuildServerPages(userCount));
        pages.Add(BuildStatEmbed("Command usage", "Command", "Used", await commandService.GetCommandsAsync(isGame: false, commandUsage), commandUsage));
        pages.Add(BuildStatEmbed("Games played", "Game", "Played", await commandService.GetCommandsAsync(isGame: true, commandUsage), commandUsage));
        pages.Add(await BuildUserPageAsync(commandUsage, forUnboxed: false, cmdUserCount));
        pages.Add(await BuildUnboxPageAsync(unboxedCount));
        pages.Add(await BuildUserPageAsync(unboxedCount, forUnboxed: true));
        pages.Add(await BuildGamblerPageAsync(gameUsage - unboxedCount));
        pages.Add(BuildStatEmbed("All loggers", "User", "Posts", await tradeLogService.GetLogStatsAsync(authors: true, logCount), logCount));
        pages.Add(BuildStatEmbed("Tradelog channels", "Channel", "Posts", await tradeLogService.GetLogStatsAsync(authors: false, logCount), logCount));
        pages.Add(await BuildItemCountPageAsync(authors: true));
        pages.Add(await BuildItemCountPageAsync(authors: false));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "equipment"], [
            "Overcharged Mixmaster", "Celestial Orbitgun", "Somnambulists Totem", "Daybreaker Band", "Asi Very High Ctr Very High", "Black Kat Cowl", "Black Kat Raiment", "Black Kat Cloak",
            "Brandish", "Combuster", "Voltedge", "Autogun", "Blitz Needle", "Electron Vortex", "Spiral Soaker", "Caladbolg"]));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "equipment"], ["Celestial Shield", "Celestial Saber", "Celestial Vortex", "Celestial Orbitgun"]));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "equipment"], ["ctr very high", "ctr high", "asi very high", "asi high", "fire max", "normal max"]));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "miscellaneous"], ["Iron Lockbox", "Mirrored Lockbox", " Titanium Lockbox", "Silver Lockbox", "Steel Lockbox", "Platinum Lockbox", "Copper Lockbox", "Gold Lockbox", "Slime Lockbox"]));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "miscellaneous"], ["Gun Pup", "Spiraltail", "Piggy", "Spiralhorn", "Snarblepup", "Love Puppy"]));
        pages.Add(await BuildTermOccurencePageAsync(["mixed-trades", "miscellaneous"], ["Extra Short Height Modifier", "Extra Tall Height Modifier", "Book of Dark Rituals", "Silver Personal Color"]));
        pages.Add(await BuildTermOccurencePageAsync([], ["Cool", "Dusky", "Fancy", "Heavy", "Military", "Regal", "Toasty"]));
        pages.Add(await BuildFindLogsPageAsync(totalSearched));
        timer.Stop();

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField("Bot Id", _client.CurrentUser.Id.ToString()),
            embedFactory.CreateField("Bot Name", _client.CurrentUser.Username),
            embedFactory.CreateField("Execution Time", $"{timer.Elapsed.TotalSeconds:F2}s"),
            embedFactory.CreateField("Running Since", $"<t:{bot.GetReadyTimestamp()}:f>"),
            embedFactory.CreateField("Round-trip Latency", $"{_client.Latency}ms"),
            embedFactory.CreateField("Commands Used", $"{commandUsage:N0}"),
            embedFactory.CreateField("Unique Bot Users", $"{cmdUserCount:N0}"),
            embedFactory.CreateField("Servers", $"{_client.Guilds.Count:N0}"),
            embedFactory.CreateField("Unique Users", $"{userCount:N0}"),
            embedFactory.CreateField("Tradelogs", $"{logCount:N0}"),
            embedFactory.CreateField("Items Searched", $"{totalSearched:N0}"),
            embedFactory.CreateField("Server Members", $"{kozmaGuild!.MemberCount:N0}"),
        };

        pages.Insert(0, embedFactory.GetBasicEmbed("General info").WithFields(fields));

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].Title = $"{pages[i].Title} - {i + 1}/{pages.Count}";
            pages[i].Footer = new EmbedFooterBuilder()
                .WithText($"{i + 1}/{pages.Count}")
                .WithIconUrl(_client.CurrentUser.GetDisplayAvatarUrl());
            
            _pages.Add(pages[i].Build());
        }

        _buildingInProgess = false;
    }

    public Embed GetPage(ulong id, string action = "")
    {
        if (_buildingInProgess) return embedFactory.GetAndBuildEmbed("Pages are being built, please try again later.");
        CheckIfIdIsPresent(id);

        switch (action)
        {
            case "first": _users[id] = 0; break;
            case "prev" when _users[id] > 0: _users[id]--; break;
            case "next" when _users[id] < _pages.Count - 1: _users[id]++; break;
            case "last": _users[id] = _pages.Count - 1; break;
            default: _users[id] = 0; break;
        }

        return _pages[_users[id]];
    }

    public MessageComponent GetComponents(ulong id)
    {
        CheckIfIdIsPresent(id);

        var page = _users[id];
        return new ComponentBuilder()
            .WithButton(label: "◀◀", customId: "stats-first", style: ButtonStyle.Primary, disabled: page == 0)
            .WithButton(label: "◀", customId: "stats-prev", style: ButtonStyle.Primary, disabled: page == 0)
            .WithButton(label: "▶", customId: "stats-next", style: ButtonStyle.Primary, disabled: page >= _pages.Count - 1)
            .WithButton(label: "▶▶", customId: "stats-last", style: ButtonStyle.Primary, disabled: page >= _pages.Count - 1)
            .Build();
    }

    private void CheckIfIdIsPresent(ulong id)
    {
        if (!_users.ContainsKey(id)) _users[id] = 0;
    }

    private async Task<int> GetUserCountAsync()
    {
        foreach (var guild in _client.Guilds)
        {
            if (!guild.HasAllMembers) await guild.DownloadUsersAsync();
        }

        return _client.Guilds
            .SelectMany(guild => guild.Users)
            .Where(member => !member.IsBot)
            .Select(member => member.Id)
            .Distinct()
            .Count();
    }

    private List<EmbedBuilder> BuildServerPages(int userCount)
    {
        var pages = new List<EmbedBuilder>();
        var serverPages = _client.Guilds
            .OrderByDescending(g => g.MemberCount)
            .Select((server, index) => $"{index + 1}. **{server.Name}**: {server.Users.Count}")
            .Select((info, index) => new { info, index })
            .GroupBy(x => x.index / 20)
            .Select(group => string.Join("\n", group.Select(x => x.info)))
            .ToList();

        int totalEmbeds = (serverPages.Count + 1) / 2;
        for (int i = 0; i < serverPages.Count; i += 2)
        {
            var embed = embedFactory.GetBasicEmbed($"Servers ({(i / 2) + 1}/{totalEmbeds})");
            var fields = new List<EmbedFieldBuilder>
            {
                embedFactory.CreateField("\u200B", serverPages[i]),
                embedFactory.CreateField("\u200B", i + 1 < serverPages.Count ? serverPages[i + 1] : "\u200B"),
                embedFactory.CreateField("\u200B", "\u200B"),
                embedFactory.CreateField("Total", $"{_client.Guilds.Count:N0}"),
                embedFactory.CreateField("Unique Users", $"{userCount:N0}")
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
            embedFactory.CreateField("User", names),
            embedFactory.CreateField(forUnboxed ? "Opened" : "Commands", counts),
            embedFactory.CreateField("Percentage", percentages)
        };
        if (totalUsers > 0) embedFactory.CreateField("Unique Users", $"{totalUsers:N0}");
        fields.Add(embedFactory.CreateField("Total", $"{totalUsed:N0}"));

        return embedFactory.GetBasicEmbed($"Top {limit} {(forUnboxed ? "unboxers" : "bot users")}").WithFields(fields);
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
            var boxData = boxHelper.GetBox(box.Name)!;

            switch (boxData.Currency)
            {
                case Enums.BoxCurrency.Energy: energy += boxData.Price; break;
                case Enums.BoxCurrency.Dollar: dollars += boxHelper.CalculateCost(box.Count, boxData); break;
            }

            boxes.AppendLine($"{index} **{box.Name}**");
            opened.AppendLine($"{box.Count:N0}");
            percentages.AppendLine($"{box.Percentage:N2}%");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField("User", boxes.ToString()),
            embedFactory.CreateField("Commands", opened.ToString()),
            embedFactory.CreateField("Percentage", percentages.ToString()),
            embedFactory.CreateField("Total", $"{total:N0}"),
            embedFactory.CreateField("Energy", $"{energy:N0}"),
            embedFactory.CreateField("Dollars", $"${dollars:N2}"),
        };

        return embedFactory.GetBasicEmbed($"Unbox command").WithFields(fields);
    }

    private async Task<EmbedBuilder> BuildGamblerPageAsync(int sessions)
    {
        var limit = 20;
        var totalSpent = await punchService.GetTotalSpentAsync();
        var data = await punchService.GetGamblersAsync(limit, totalSpent);
        var (names, counts, percentages) = GetBasicFieldValues(data);

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField("User", names),
            embedFactory.CreateField("Crowns Spent", counts),
            embedFactory.CreateField("Percentage", percentages),
            embedFactory.CreateField("Total", $"{totalSpent:N0}"),
            embedFactory.CreateField("Sessions", $"{sessions:N0}")
        };

        return embedFactory.GetBasicEmbed($"Top {limit} highest spenders at Punch").WithFields(fields);
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
            names.AppendLine($"{index} **{item.Item}**");
            searches.AppendLine($"{item.Count:N0}");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField("Item", names.ToString()),
            embedFactory.CreateField("Searches", searches.ToString()),
            embedFactory.CreateField("Unique Searches", $"{totalSearched:N0}", inline: false),
        };

        return embedFactory.GetBasicEmbed($"Top {limit} searched items").WithFields(fields);
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
            names.AppendLine($"{index} **{term.Key}**");
            searches.AppendLine($"{term.Value:N0}");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField("Logged", names.ToString()),
            embedFactory.CreateField("Count", searches.ToString()),
            embedFactory.CreateField("Total", $"{total:N0}", inline: false),
        };

        return embedFactory.GetBasicEmbed("Estimated logged items").WithFields(fields);
    }

    private EmbedBuilder BuildStatEmbed(string title, string field1, string field2, IEnumerable<DbStat> data, int total)
    {
        var (names, counts, percentages) = GetBasicFieldValues(data);
        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField(field1, names),
            embedFactory.CreateField(field2, counts),
            embedFactory.CreateField("Percentage", percentages),
            embedFactory.CreateField("Total", $"{total:N0}"),
        };

        return embedFactory.GetBasicEmbed(title).WithFields(fields);
    }

    private static (string names, string counts, string percentages) GetBasicFieldValues(IEnumerable<DbStat> data)
    {
        var names = new StringBuilder();
        var counts = new StringBuilder();
        var percentages = new StringBuilder();

        var index = 1;
        foreach (var item in data)
        {
            names.AppendLine($"{index} **{item.Name}**");
            counts.AppendLine($"{item.Count:N0}");
            percentages.AppendLine($"{item.Percentage:N2}%");
            index++;
        }

        return (names.ToString(), counts.ToString(), percentages.ToString());
    }
}
