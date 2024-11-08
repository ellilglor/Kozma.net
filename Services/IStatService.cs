using Kozma.net.Models;

namespace Kozma.net.Services;

public interface IStatService
{
    public Task<int> GetCommandUsageAsync(bool isGame);
    public Task<IEnumerable<CommandStats>> GetCommandsAsync(bool isGame, int total);
}
