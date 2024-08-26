using Kozma.net.Models;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Services;

public class TradeLogService(KozmaDbContext dbContext) : ITradeLogService
{
    public async Task<IEnumerable<LogCollection>> GetLogsAsync(string item, DateTime date, bool checkMixed, bool skipSpecial, string? ignore)
    {
        var query = dbContext.TradeLogs
            .Where(l => l.Content.Contains(item) && l.Date > date);

        if (!checkMixed)
        {
            query = query.Where(l => l.Channel != "mixed-trades");
        }

        if (skipSpecial)
        {
            query = query.Where(l => l.Channel != "special-listings");
        }

        if (!string.IsNullOrEmpty(ignore) && ignore.Length > 2)
        {
            query = query.Where(l => !EF.Functions.Like(l.Content, $"%{ignore}%"));
        }

        var logs = await query
            .OrderByDescending(l => l.Date)
            .GroupBy(l => l.Channel)
            .Select(g => new LogCollection
            {
                Channel = g.Key,
                Messages = g.OrderByDescending(l => l.Date).ToList()
            })
            .OrderByDescending(g => g.Channel)
            .ToListAsync();

        return logs;
    }
}
