using Kozma.net.Models;

namespace Kozma.net.Services;

public interface ITradeLogService
{
    Task<IEnumerable<LogCollection>> GetLogsAsync(string item, DateTime date, bool checkMixed, bool skipSpecial, string? ignore);
}
