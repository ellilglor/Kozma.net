using Kozma.net.Models;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Services;

public class UserService(KozmaDbContext dbContext) : IUserService
{
    public async Task<int> GetTotalUsersCountAsync()
    {
        return await dbContext.Users.CountAsync();
    }

    public async Task<IEnumerable<DbStat>> GetUsersAsync(int limit, int total, bool forUnboxed)
    {
        var query = await dbContext.Users
            .OrderByDescending(u => forUnboxed ? u.Unboxed : u.Count)
            .ThenBy(u => u.Name)
            .Take(limit)
            .ToListAsync();

        return query.Select(u => new DbStat(u.Name, forUnboxed ? u.Unboxed : u.Count, (forUnboxed ? u.Unboxed : u.Count) / (double)total));
    }
}
