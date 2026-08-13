using Kozma.net.Src.Data.Constants;
using Kozma.net.Src.Interfaces.Services;
using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Src.Services;

public class UserService(KozmaDbContext dbContext) : IUserService
{
    public async Task UpdateOrSaveUserAsync(ulong id, string name, bool isCommand, string command)
    {
        if (id == Data.Constants.Ids.Owner) return;

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            await dbContext.Users.AddAsync(new User()
            {
                Id = id,
                Name = name,
                Commands = isCommand ? 1 : 0,
                Unboxed = isCommand ? 0 : command == CommandIds.Unbox ? 1 : 0,
                Punched = isCommand ? 0 : command == CommandIds.Punch ? 1 : 0,
                ShardSwept = isCommand ? 0 : command == CommandIds.ShardSweeper ? 1 : 0,
            });
        }
        else
        {
            switch (command)
            {
                case CommandIds.Unbox: user.Unboxed++; break;
                case CommandIds.Punch: user.Punched++; break;
                case CommandIds.ShardSweeper: user.ShardSwept++; break;
                default: user.Commands++; break;
            }

            if (user.Name != name) user.Name = name; // Get rid of legacy discord tag

            dbContext.Users.Update(user);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<int> GetTotalUsersCountAsync() =>
        await dbContext.Users.CountAsync();

    public async Task<IEnumerable<DbStat>> GetUsersAsync(int limit, int total, bool forUnboxed)
    {
        var query = await dbContext.Users
            .OrderByDescending(u => forUnboxed ? u.Unboxed : u.Commands)
            .ThenBy(u => u.Name)
            .Take(limit)
            .ToListAsync();

        return query.Select(u => new DbStat(u.Name, forUnboxed ? u.Unboxed : u.Commands, (forUnboxed ? u.Unboxed : u.Commands) / (double)total));
    }
}
