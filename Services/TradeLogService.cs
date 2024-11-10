﻿using Kozma.net.Helpers;
using Kozma.net.Models;
using Kozma.net.Models.Database;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using System.Text.RegularExpressions;

namespace Kozma.net.Services;

public class TradeLogService(KozmaDbContext dbContext, IFileReader jsonFileReader) : ITradeLogService
{
    public async Task<IEnumerable<LogCollection>> GetLogsAsync(List<string> items, DateTime date, bool checkMixed, bool skipSpecial, List<string> ignore)
    {
        var query = dbContext.TradeLogs.Where(log => items.Any(item => log.Content.Contains(item)) && log.Date > date);
        if (!checkMixed) query = query.Where(log => log.Channel != "mixed-trades");
        if (skipSpecial) query = query.Where(log => log.Channel != "special-listings");
        if (ignore.Count > 0) query = query.Where(log => ignore.All(item => !log.Content.Contains(item)));

        // Grouping isn't supported at this moment so have to query in 2 steps
        var queried = await query
            .OrderByDescending(log => log.Date)
            .ToListAsync();

        var logs = queried
            .GroupBy(l => l.Channel)
            .Select(g => new LogCollection
            {
                Channel = g.Key,
                Messages = g.OrderByDescending(l => l.Date).ToList()
            })
            .OrderByDescending(g => g.Channel)
            .ToList();

        return logs;
    }

    public async Task UpdateLogsAsync(List<TradeLog> logs, bool reset = false, string? channel = null)
    {
        if (reset && !string.IsNullOrEmpty(channel)) await DeleteLogsAsync(channel);

        await dbContext.TradeLogs.AddRangeAsync(logs);
        await dbContext.SaveChangesAsync();
    }

    private async Task DeleteLogsAsync(string channel)
    {
        var toDelete = await dbContext.TradeLogs.Where(log => log.Channel == channel).ToListAsync();
        dbContext.TradeLogs.RemoveRange(toDelete);
        await dbContext.SaveChangesAsync();
    }

    public async Task<int> GetTotalLogCountAsync()
    {
        return await dbContext.TradeLogs.CountAsync();
    }

    public async Task<IEnumerable<DbStat>> GetLogStatsAsync(bool authors, int total)
    {
        var query = await dbContext.TradeLogs.ToListAsync();

        return query
            .GroupBy(l => authors ? l.Author : l.Channel)
            .Select(g => new DbStat(g.Key, g.Count(), Math.Round(g.Count() / (double)total * 100, 2)))
            .OrderByDescending(g => g.Count)
            .ToList();
    }

    public async Task<(IEnumerable<DbStat> Stats, int Total)> GetItemCountAsync(bool authors)
    {
        var fileNames = new List<string>() { "Equipments", "Costumes", "HelmTops", "HelmFronts", "HelmBacks", "HelmSides", "ArmorFronts", "ArmorBacks", "ArmorRears", "ArmorAnkles", "Auras", "Miscellaneous" };
        var ignore = new List<string>() { "special-listings", "2023-flash-sales", "2022-flash-sales", "2021-flash-sales", "2020-flash-sales" };
        var query = await dbContext.TradeLogs.Where(l => !ignore.Contains(l.Channel)).ToListAsync();
        var channels = query.GroupBy(l => authors ? l.Author : l.Channel).ToList();
        var counts = new Dictionary<string, int>();
        var totalCount = 0;

        foreach (var group in channels)
        {
            var itemCount = 0;

            if (authors || group.Key == "mixed-trades")
            {
                foreach (var name in fileNames)
                {
                    itemCount += await CountItemsAsync(name, group);
                }
            } else
            {
                itemCount = await CountItemsAsync(ConvertToFileName(group.Key), group);
            }

            totalCount += itemCount;
            counts[group.Key] = itemCount;
        }

        var data = counts
            .Select(x => new DbStat(x.Key, x.Value, Math.Round(x.Value / (double)totalCount * 100, 2)))
            .OrderByDescending(x => x.Count)
            .ToList();

        return (data, totalCount);
    }

    public async Task<int> GetTotalSearchCountAsync()
    {
        return await dbContext.SearchedLogs.CountAsync();
    }

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
            "equipment" => "Equipments",
            "costumes" => "Costumes",
            "helm-top" => "HelmTops",
            "helm-front" => "HelmFronts",
            "helm-back" => "HelmBacks",
            "helm-side" => "HelmSides",
            "armor-front" => "ArmorFronts",
            "armor-back" => "ArmorBacks",
            "armor-rear" => "ArmorRears",
            "armor-ankle" => "ArmorAnkles",
            "armor-aura" => "Auras",
            "miscellaneous" => "Miscellaneous",
            "Sprite Food" => "Miscellaneous",
            "Materials" => "Miscellaneous",
            _ => string.Empty
        };
    }

    private async Task<int> CountItemsAsync(string fileName, IGrouping<string, TradeLog> group)
    {
        var items = await GetItemListAsync(fileName);
        var itemCount = 0;

        foreach (var message in group)
        {
            foreach (var item in items)
            {
                itemCount += Regex.Matches(message.OriginalContent, Regex.Escape(item), RegexOptions.IgnoreCase).Count;
            }
        }

        return itemCount;
    }

    private async Task<List<string>> GetItemListAsync(string channel)
    {
        var projectRoot = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
        if (projectRoot == null) return [];

        var directory = Path.Combine(projectRoot, "Data", "Items", $"{channel}.json");
        var items = await jsonFileReader.ReadAsync<List<string>>(directory);

        return items ?? [];
    }
}
