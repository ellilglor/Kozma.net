using Kozma.net.Src.Helpers;
using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace Kozma.net.Src.Services;

public class TradeLogService(KozmaDbContext dbContext, IFileReader jsonFileReader) : ITradeLogService
{
    public bool LogsAreBeingReset { get; private set; }

    public async Task UpdateOrSaveItemAsync(string item)
    {
        var model = await dbContext.SearchedLogs.FirstOrDefaultAsync(i => i.Item == item);

        if (model is null)
        {
            await dbContext.SearchedLogs.AddAsync(new SearchedLog()
            {
                Id = ObjectId.GenerateNewId(),
                Item = item,
                Count = 1
            });
        }
        else
        {
            model.Count++;
            dbContext.SearchedLogs.Update(model);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<LogGroups>> GetLogsAsync(IReadOnlyCollection<string> items, DateTime dateToCompare, bool checkMixed, bool skipSpecial, IReadOnlyCollection<string> ignore)
    {
#pragma warning disable CA1307 // Specify StringComparison for clarity -> mongodb driver doesn't support StringComparison.OrdinalIgnoreCase
        var query = dbContext.TradeLogs.Where(log => items.Any(item => log.Content.Contains(item)) && log.Date > dateToCompare);
        if (!checkMixed) query = query.Where(log => log.Channel != "mixed-trades");
        if (skipSpecial) query = query.Where(log => log.Channel != "special-listings");
        if (ignore.Count > 0) query = query.Where(log => ignore.All(item => !log.Content.Contains(item)));
#pragma warning restore CA1307 // Specify StringComparison for clarity

        // Grouping isn't supported at this moment so have to query in 2 steps
        var queried = await query
            .OrderByDescending(log => log.Date)
            .ToListAsync();

        var logs = queried
            .GroupBy(l => l.Channel)
            .Select(g => new LogGroups
            {
                Channel = g.Key,
                Messages = g.OrderByDescending(l => l.Date).ToList()
            })
            .OrderByDescending(g => g.Channel)
            .ToList();

        return logs;
    }

    public async Task UpdateLogsAsync(IReadOnlyCollection<TradeLog> logs)
    {
        await dbContext.TradeLogs.AddRangeAsync(logs);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAndUpdateLogsAsync(IReadOnlyCollection<TradeLog> logs)
    {
        LogsAreBeingReset = true;
        dbContext.TradeLogs.RemoveRange(await dbContext.TradeLogs.ToListAsync());
        await dbContext.SaveChangesAsync();
        // save twice because of duplicate keys
        await dbContext.TradeLogs.AddRangeAsync(logs);
        await dbContext.SaveChangesAsync();
        LogsAreBeingReset = false;
    }

    public async Task<bool> CheckIfLogExistsAsync(ulong id) =>
        await dbContext.TradeLogs.FirstOrDefaultAsync(log => log.Id == id.ToString()) != null;

    public async Task<int> GetTotalLogCountAsync() =>
        await dbContext.TradeLogs.CountAsync();

    public async Task<IEnumerable<DbStat>> GetLogStatsAsync(bool authors, int total)
    {
        var query = await dbContext.TradeLogs.ToListAsync();

        return query
            .GroupBy(l => authors ? l.Author : l.Channel)
            .Select(g => new DbStat(g.Key, g.Count(), g.Count() / (double)total))
            .OrderByDescending(g => g.Count)
            .ToList();
    }

    public async Task<(IEnumerable<DbStat> Stats, int Total)> GetItemCountAsync(bool authors)
    {
        var query = await dbContext.TradeLogs.Where(l => l.Channel != "special-listings" && !l.Channel.Contains("flash-sales")).ToListAsync();
        var channels = query.GroupBy(l => authors ? l.Author : l.Channel).ToList();
        var regexCache = new Dictionary<string, Regex>();
        var counts = new Dictionary<string, int>();
        var totalCount = 0;

        var folder = Directory.GetFiles("Src/Data/Items");
        foreach (var file in folder)
        {
            var fileName = Path.GetFileName(file);
            var items = await jsonFileReader.ReadAsync<IEnumerable<string>>(Path.Combine("Data", "Items", fileName));
            regexCache[fileName] = new Regex(string.Join("|", items.Select(Regex.Escape)), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        foreach (var group in channels)
        {
            var itemCount = 0;

            if (authors || group.Key == "mixed-trades")
            {
                foreach (var regex in regexCache)
                {
                    itemCount += group.AsParallel().Sum(message => regex.Value.Matches(message.OriginalContent).Count);
                }
            }
            else
            {
                itemCount = group.AsParallel().Sum(message => regexCache[ConvertToFileName(group.Key)].Matches(message.OriginalContent).Count);
            }

            totalCount += itemCount;
            counts[group.Key] = itemCount;
        }

        var data = counts
            .Select(x => new DbStat(x.Key, x.Value, x.Value / (double)totalCount))
            .OrderByDescending(x => x.Count)
            .ToList();

        return (data, totalCount);
    }

    public async Task<(IOrderedEnumerable<KeyValuePair<string, int>>, int Total)> CountOccurencesAsync(IReadOnlyCollection<string> channels, IReadOnlyCollection<string> terms)
    {
        var messages = await dbContext.TradeLogs.Where(l => channels.Count == 0 || channels.Contains(l.Channel)).ToListAsync();
        var counts = new Dictionary<string, int>();
        var totalCount = 0;

        foreach (var term in terms)
        {
            var regex = new Regex(Regex.Escape(term), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var count = messages.AsParallel().Sum(msg => regex.Matches(msg.Content).Count);

            totalCount += count;
            counts[term] = count;
        }

        var data = counts.OrderByDescending(x => x.Value);
        return (data, totalCount);
    }

    public async Task<int> GetTotalSearchCountAsync() =>
        await dbContext.SearchedLogs.CountAsync();

    public async Task<IEnumerable<SearchedLog>> GetSearchedLogsAsync(int limit)
    {
        return await dbContext.SearchedLogs
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Item)
            .Take(limit)
            .ToListAsync();
    }

    private static string ConvertToFileName(string channel)
    {
        return channel switch
        {
            "equipment" => "Equipments.json",
            "costumes" => "Costumes.json",
            "helm-top" => "HelmTops.json",
            "helm-front" => "HelmFronts.json",
            "helm-back" => "HelmBacks.json",
            "helm-side" => "HelmSides.json",
            "armor-front" => "ArmorFronts.json",
            "armor-back" => "ArmorBacks.json",
            "armor-rear" => "ArmorRears.json",
            "armor-ankle" => "ArmorAnkles.json",
            "armor-aura" => "Auras.json",
            "miscellaneous" => "Miscellaneous.json",
            "Sprite Food" => "Miscellaneous.json",
            "Materials" => "Miscellaneous.json",
            _ => string.Empty
        };
    }
}
