using Discord;
using Discord.WebSocket;
using Kozma.net.Factories;
using Kozma.net.Helpers;
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

    public async Task BuildPagesAsync()
    {
        _pages = [];
        var pages = new List<EmbedBuilder>();

        var userCount = await GetUserCountAsync();
        var commandUsage = await commandService.GetCommandUsageAsync(isGame: false);
        var gameUsage = await commandService.GetCommandUsageAsync(isGame: true);
        var cmdUserCount = await userService.GetTotalUsersCountAsync();
        var unboxedCount = await unboxService.GetBoxOpenedCountAsync();
        var logCount = await tradeLogService.GetTotalLogCountAsync();
        var totalSearched = await tradeLogService.GetTotalSearchCountAsync();
        var kozmaGuild = _client.Guilds.FirstOrDefault(g => g.Id.Equals(config.GetValue<ulong>("ids:serverId")));

        pages.AddRange(BuildServerPages(userCount));
        pages.Add(await BuildCommandPageAsync(games: false, commandUsage));
        pages.Add(await BuildCommandPageAsync(games: true, gameUsage));
        pages.Add(await BuildUserPageAsync(commandUsage, forUnboxed: false, cmdUserCount));
        pages.Add(await BuildUnboxPageAsync(unboxedCount));
        pages.Add(await BuildUserPageAsync(unboxedCount, forUnboxed: true));
        pages.Add(await BuildGamblerPageAsync(gameUsage - unboxedCount));
        pages.Add(await BuildLogsPageAsync(logCount, authors: true));
        pages.Add(await BuildLogsPageAsync(logCount, authors: false));
        pages.Add(await BuildFindLogsPageAsync(totalSearched));

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField("Bot Tag", _client.CurrentUser.Username),
            embedFactory.CreateField("Bot Id", _client.CurrentUser.Id.ToString()),
            embedFactory.CreateField("Round-trip Latency", $"{_client.Latency}ms"),
            embedFactory.CreateField("Running Since", $"<t:{bot.GetReadyTimestamp()}:f>"),
            embedFactory.CreateField("Commands Used", $"{commandUsage:N0}"),
            embedFactory.CreateField("Unique Bot Users", $"{cmdUserCount:N0}"),
            embedFactory.CreateField("Servers", $"{_client.Guilds.Count:N0}"),
            embedFactory.CreateField("Unique Users", $"{userCount:N0}"),
            embedFactory.CreateField("\u200B", "\u200B"),
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
    }

    public Embed GetPage(ulong id, string action = "")
    {
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

    private async Task<EmbedBuilder> BuildCommandPageAsync(bool games, int total)
    {
        var data = await commandService.GetCommandsAsync(games, total);
        var names = new StringBuilder();
        var amounts = new StringBuilder();
        var percentages = new StringBuilder();

        var index = 1;
        foreach (var cmd in data)
        {
            names.AppendLine($"{index} **{cmd.Command.Name}**");
            amounts.AppendLine($"{cmd.Command.Count:N0}");
            percentages.AppendLine($"{cmd.Percentage:N2}%");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField(games ? "Game" : "Command", names.ToString()),
            embedFactory.CreateField("Used", amounts.ToString()),
            embedFactory.CreateField("Percentage", percentages.ToString()),
            embedFactory.CreateField("Total", $"{total:N0}"),
        };

        return embedFactory.GetBasicEmbed(games ? "Games played" : "Command usage").WithFields(fields);
    }

    private async Task<EmbedBuilder> BuildUserPageAsync(int totalUsed, bool forUnboxed, int totalUsers = 0)
    {
        var limit = 20;
        var data = await userService.GetUsersAsync(limit, totalUsed, forUnboxed);
        var names = new StringBuilder();
        var amounts = new StringBuilder();
        var percentages = new StringBuilder();

        var index = 1;
        foreach (var user in data)
        {
            names.AppendLine($"{index} **{user.User.Name}**");
            amounts.AppendLine(forUnboxed ? $"{user.User.Unboxed:N0}" : $"{user.User.Count:N0}");
            percentages.AppendLine($"{user.Percentage:N2}%");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField("User", names.ToString()),
            embedFactory.CreateField(forUnboxed ? "Opened" : "Commands", amounts.ToString()),
            embedFactory.CreateField("Percentage", percentages.ToString())
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
            var boxData = boxHelper.GetBox(box.Box.Name)!;

            switch (boxData.Currency)
            {
                case Enums.BoxCurrency.Energy: energy += boxData.Price; break;
                case Enums.BoxCurrency.Dollar: dollars += boxHelper.CalculateCost(box.Box.Count, boxData); break;
            }

            boxes.AppendLine($"{index} **{box.Box.Name}**");
            opened.AppendLine($"{box.Box.Count:N0}");
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
        var names = new StringBuilder();
        var spent = new StringBuilder();
        var percentages = new StringBuilder();

        var index = 1;
        foreach (var user in data)
        {
            names.AppendLine($"{index} **{user.Gambler.Name}**");
            spent.AppendLine($"{user.Gambler.Total:N0}");
            percentages.AppendLine($"{user.Percentage:N2}%");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField("User", names.ToString()),
            embedFactory.CreateField("Crowns Spent", spent.ToString()),
            embedFactory.CreateField("Percentage", percentages.ToString()),
            embedFactory.CreateField("Total", $"{totalSpent:N0}"),
            embedFactory.CreateField("Sessions", $"{sessions:N0}")
        };

        return embedFactory.GetBasicEmbed($"Top {limit} highest spenders at Punch").WithFields(fields);
    }

    private async Task<EmbedBuilder> BuildLogsPageAsync(int total, bool authors)
    {
        var data = await tradeLogService.GetLogStatsAsync(authors, total);
        var names = new StringBuilder();
        var posts = new StringBuilder();
        var percentages = new StringBuilder();

        var index = 1;
        foreach (var group in data)
        {
            names.AppendLine($"{index} **{group.Name}**");
            posts.AppendLine($"{group.Count:N0}");
            percentages.AppendLine($"{group.Percentage:N2}%");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField(authors ? "User" : "Channel", names.ToString()),
            embedFactory.CreateField("Posts", posts.ToString()),
            embedFactory.CreateField("Percentage", percentages.ToString()),
            embedFactory.CreateField("Total", $"{total:N0}"),
        };

        return embedFactory.GetBasicEmbed(authors ? "All loggers" : "Tradelog channels").WithFields(fields);
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
}
