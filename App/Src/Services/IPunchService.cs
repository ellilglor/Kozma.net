using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;

namespace Kozma.net.Src.Services;

public interface IPunchService
{
    Task UpdateOrSaveGamblerAsync(ulong id, string name, PunchPrices ticket);
    Task<long> GetTotalSpentAsync();
    Task<IEnumerable<DbStat>> GetGamblersAsync(int limit, long total);
}
