using Kozma.net.Models;
using Kozma.net.Models.Database;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;

namespace Kozma.net.Services;

public class TradeLogService(KozmaDbContext dbContext) : ITradeLogService
{
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
}
