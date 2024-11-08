using Discord;
using Discord.WebSocket;
using Kozma.net.Factories;
using Kozma.net.Services;
using System.Text;

namespace Kozma.net.Trackers;

public class StatPageTracker(IBot bot,
    IEmbedFactory embedFactory,
    ICommandService commandService,
    IUserService userService) : IStatPageTracker
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
        var cmdUserCount = await userService.GetTotalUsersCountAsync();

        pages.AddRange(BuildServerPages(userCount));
        pages.Add(await BuildCommandPageAsync(games: false, commandUsage));
        pages.Add(await BuildCommandPageAsync(games: true, await commandService.GetCommandUsageAsync(isGame: true)));
        pages.Add(await BuilduserPageAsync(commandUsage, cmdUserCount));

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].Title = $"{pages[i].Title} - {i + 1}/{pages.Count}";
            _pages.Add(pages[i].Build());
        }
    }

    public Embed GetPage(ulong id, string action = "")
    {
        CheckIfIdIsPresent(id);

        switch (action)
        {
            case "first": _users[id] = 0; break;
            case "prev": _users[id]--; break;
            case "next": _users[id]++; break;
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
        /*foreach (var guild in _client.Guilds)
        {
            await guild.DownloadUsersAsync();
        }*/

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
        var emptyField = embedFactory.CreateField("\u200B", "\u200B");

        var serverPages = _client.Guilds
            .OrderByDescending(g => g.MemberCount)
            .Select((server, index) => $"{index + 1}. **{server.Name}**: {server.Users.Count}")
            .Select((info, index) => new { info, index })
            .GroupBy(x => x.index / 20)
            .Select(group => string.Join("\n", group.Select(x => x.info)))
            .ToList();

        for (int i = 0; i < serverPages.Count; i += 2)
        {
            var embed = embedFactory.GetEmbed($"Servers ({(i / 2) + 1}/{(serverPages.Count + 1) / 2})");
            var fields = new List<EmbedFieldBuilder>
            {
                embedFactory.CreateField("\u200B", serverPages[i]),
                i + 1 < serverPages.Count ? embedFactory.CreateField("\u200B", serverPages[i + 1]) : emptyField,
                emptyField,
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

        return embedFactory.GetEmbed(games ? "Games Played" : "Command Usage").WithFields(fields);
    }

    private async Task<EmbedBuilder> BuilduserPageAsync(int commandsUsed, int totalUsers)
    {
        var limit = 20;
        var data = await userService.GetUsersAsync(limit, commandsUsed);

        var names = new StringBuilder();
        var amounts = new StringBuilder();
        var percentages = new StringBuilder();

        var index = 1;
        foreach (var user in data)
        {
            names.AppendLine($"{index} **{user.User.Name}**");
            amounts.AppendLine($"{user.User.Count:N0}");
            percentages.AppendLine($"{user.Percentage:N2}%");
            index++;
        }

        var fields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField("User", names.ToString()),
            embedFactory.CreateField("Commands", amounts.ToString()),
            embedFactory.CreateField("Percentage", percentages.ToString()),
            embedFactory.CreateField("Unique Users", $"{totalUsers:N0}"),
            embedFactory.CreateField("Total", $"{commandsUsed:N0}"),
        };

        return embedFactory.GetEmbed($"Top {limit} bot users").WithFields(fields);
    }
}
