using Kozma.net.Src.Models;

namespace Kozma.net.Src.Services;

public interface IUserService
{
    public Task<int> GetTotalUsersCountAsync();
    public Task<IEnumerable<DbStat>> GetUsersAsync(int limit, int total, bool forUnboxed);
}
