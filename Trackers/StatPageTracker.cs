using Discord;
using Discord.WebSocket;
using Kozma.net.Factories;

namespace Kozma.net.Trackers;

public class StatPageTracker(IBot bot, IEmbedFactory embedFactory) : IStatPageTracker
{
    private readonly DiscordSocketClient _client = bot.GetClient();
    private List<Embed> _pages = [];

    public async Task BuildPagesAsync()
    {
        _pages = [];
        var pages = new List<EmbedBuilder>();
        
        var userCount = await GetUserCountAsync();
        pages.AddRange(BuildServerPages(userCount));

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].Title = $"{pages[i].Title} - {i + 1}/{pages.Count}";
            _pages.Add(pages[i].Build());
        }
    }

    public Embed GetPage()
    {
        return _pages[0];
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

        var serverPages = _client.Guilds
            .OrderByDescending(g => g.MemberCount)
            .Select((server, index) => $"{index + 1}. **{server.Name}**: {server.Users.Count}")
            .Select((info, index) => new { info, index })
            .GroupBy(x => x.index / 20)
            .Select(group => string.Join("\n", group.Select(x => x.info)))
            .ToList();

        var basicFields = new List<EmbedFieldBuilder>()
        {
            embedFactory.CreateField("\u200B", "\u200B"),
            embedFactory.CreateField("Total", $"{_client.Guilds.Count:N0}"),
            embedFactory.CreateField("Unique Users", $"{userCount:N0}")
        };

        for (int i = 0; i < serverPages.Count; i += 2)
        {
            var embed = embedFactory.GetEmbed($"Servers ({(i / 2) + 1}/{(serverPages.Count + 1) / 2})");
            var fields = new List<EmbedFieldBuilder>()
            {
                embedFactory.CreateField("\u200B", serverPages[i]),
            };

            if (i + 1 < serverPages.Count) fields.Add(embedFactory.CreateField("\u200B", serverPages[i + 1]));
            fields.AddRange(basicFields);

            pages.Add(embed.WithFields(fields));
        }

        return pages;
    }
}
