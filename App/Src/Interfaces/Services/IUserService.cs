using Kozma.net.Src.Models;

namespace Kozma.net.Src.Interfaces.Services;

public interface IUserService
{
    Task UpdateOrSaveUserAsync(ulong id, string name, bool isCommand, string command);
    Task<int> GetTotalUsersCountAsync();
    Task<IEnumerable<DbStat>> GetUsersAsync(int limit, int total, bool forUnboxed);
}
