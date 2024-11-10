using Kozma.net.Models;

namespace Kozma.net.Services;

public interface IUserService
{
    public Task<int> GetTotalUsersCountAsync();
    public Task<IEnumerable<DbStat>> GetUsersAsync(int limit, int total, bool forUnboxed);
}
