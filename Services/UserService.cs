using Kozma.net.Models;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Services;

public class UserService(KozmaDbContext dbContext) : IUserService
{
    public async Task<int> GetTotalUsersCountAsync()
    {
        return await dbContext.Users.CountAsync();
    }

    public async Task<IEnumerable<UserStats>> GetUsersAsync(int limit, int total)
    {
        try
        {
            var query = await dbContext.Users
            .OrderByDescending(u => u.Count)
            .ThenBy(u => u.Name)
            .Take(limit)
            .ToListAsync();

            return query.Select(u => new UserStats(u, Math.Round((u.Count / (double)total) * 100, 2)));
        }
        catch (Exception ex)
        {

        }
        var test = await dbContext.Users
            .OrderByDescending(u => u.Count)
            .ThenBy(u => u.Name)
            .Take(limit)
            .ToListAsync();

        return test.Select(u => new UserStats(u, Math.Round((u.Count / (double)total) * 100, 2)));
    }
}
