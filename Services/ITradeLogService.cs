using Kozma.net.Models;

namespace Kozma.net.Services;

public interface ITradeLogService
{
    Task<int> GetTotalLogCountAsync();
    Task<IEnumerable<TradeLogStats>> GetLogStatsAsync(bool authors, int total);
    Task<IEnumerable<LogCollection>> GetLogsAsync(List<string> items, DateTime date, bool checkMixed, bool skipSpecial, List<string> ignore);
    Task UpdateLogsAsync(List<TradeLog> logs, bool reset = false, string? channel = null);
}
