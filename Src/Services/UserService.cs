using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Src.Services;

public class UserService(KozmaDbContext dbContext) : IUserService
{
    public async Task UpdateOrAddUserAsync(ulong id, string name, bool isCommand, bool isUnbox)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id.ToString());

        if (user is null)
        {
            await dbContext.Users.AddAsync(new User()
            {
                Id = id.ToString(),
                Name = name,
                Count = isCommand ? 1 : 0,
                Unboxed = isCommand ? 0 : isUnbox ? 1 : 0,
                Punched = isCommand ? 0 : isUnbox ? 0 : 1,
            });
        } 
        else
        {
            if (isCommand) user.Count++;
            else if (isUnbox) user.Unboxed++;
            else user.Punched++;
            dbContext.Users.Update(user);
        }

        await dbContext.SaveChangesAsync();
    }

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
