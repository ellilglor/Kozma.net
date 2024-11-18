using Kozma.net.Src.Models.Database;

namespace Kozma.net.Src.Services;

public interface IUnboxService
{
    public Task<int> GetBoxOpenedCountAsync();
    public Task<IEnumerable<UnboxStat>> GetBoxesAsync(int total);
}
