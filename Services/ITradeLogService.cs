using Kozma.net.Models;
using Kozma.net.Models.Database;

namespace Kozma.net.Services;

public interface ITradeLogService
{
    Task<int> GetTotalLogCountAsync();
    Task<IEnumerable<DbStat>> GetLogStatsAsync(bool authors, int total);
    Task<IEnumerable<LogCollection>> GetLogsAsync(List<string> items, DateTime date, bool checkMixed, bool skipSpecial, List<string> ignore);
    Task UpdateLogsAsync(List<TradeLog> logs, bool reset = false, string? channel = null);
    Task<(IOrderedEnumerable<KeyValuePair<string, int>>, int Total)> CountOccurencesAsync(List<string> channels, List<string> terms);
    Task<int> GetTotalSearchCountAsync();
    Task<IEnumerable<SearchedLog>> GetSearchedLogsAsync(int limit);
    Task<(IEnumerable<DbStat> Stats, int Total)> GetItemCountAsync(bool authors);
}
