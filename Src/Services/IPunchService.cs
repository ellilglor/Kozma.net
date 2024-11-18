using Kozma.net.Src.Models;

namespace Kozma.net.Src.Services;

public interface IPunchService
{
    public Task<long> GetTotalSpentAsync();
    public Task<IEnumerable<DbStat>> GetGamblersAsync(int limit, long total);
}
