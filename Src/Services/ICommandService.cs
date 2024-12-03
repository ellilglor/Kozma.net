using Kozma.net.Src.Models;

namespace Kozma.net.Src.Services;

public interface ICommandService
{
    Task UpdateOrSaveCommandAsync(string name, bool isCommand = true);
    Task<int> GetCommandUsageAsync(bool isGame);
    Task<IEnumerable<DbStat>> GetCommandsAsync(bool isGame, int total);
}
