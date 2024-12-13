using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Services;

public interface ITradeLogService
{
    Task UpdateOrSaveItemAsync(string item);
    Task<int> GetTotalLogCountAsync();
    Task<IEnumerable<DbStat>> GetLogStatsAsync(bool authors, int total);
    Task<IEnumerable<LogGroups>> GetLogsAsync(IReadOnlyCollection<string> items, DateTime dateToCompare, bool checkMixed, bool skipSpecial, IReadOnlyCollection<string> ignore);
    Task UpdateLogsAsync(IReadOnlyCollection<TradeLog> logs, bool reset = false, string? channel = null);
    Task<bool> CheckIfLogExistsAsync(ulong id);
    Task<(IOrderedEnumerable<KeyValuePair<string, int>>, int Total)> CountOccurencesAsync(IReadOnlyCollection<string> channels, IReadOnlyCollection<string> terms);
    Task<int> GetTotalSearchCountAsync();
    Task<IEnumerable<SearchedLog>> GetSearchedLogsAsync(int limit);
    Task<(IEnumerable<DbStat> Stats, int Total)> GetItemCountAsync(bool authors);
}
