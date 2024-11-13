using Kozma.net.Models.Database;

namespace Kozma.net.Services;

public interface IUnboxService
{
    public Task<int> GetBoxOpenedCountAsync();
    public Task<IEnumerable<UnboxStat>> GetBoxesAsync(int total);
}
