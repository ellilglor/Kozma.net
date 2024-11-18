using Kozma.net.Src.Models;

namespace Kozma.net.Src.Services;

public interface ICommandService
{
    public Task UpdateOrAddCommandAsync(string name, bool isCommand = true);
    public Task<int> GetCommandUsageAsync(bool isGame);
    public Task<IEnumerable<DbStat>> GetCommandsAsync(bool isGame, int total);
}
