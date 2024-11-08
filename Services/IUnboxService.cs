using Kozma.net.Models;

namespace Kozma.net.Services;

public interface IUnboxService
{
    public Task<int> GetBoxOpenedCountAsync();
    public Task<IEnumerable<BoxStats>> GetBoxesAsync(int total);
}
