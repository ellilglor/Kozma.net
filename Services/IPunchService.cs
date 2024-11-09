using Kozma.net.Models;

namespace Kozma.net.Services;

public interface IPunchService
{
    public Task<long> GetTotalSpentAsync();
    public Task<IEnumerable<GamblerStats>> GetGamblersAsync(int limit, long total);
}
