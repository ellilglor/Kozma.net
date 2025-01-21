using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Services;

public class UserService(KozmaDbContext dbContext, IConfiguration config) : IUserService
{
    public async Task UpdateOrSaveUserAsync(ulong id, string name, bool isCommand, string command)
    {
        if (id == config.GetValue<ulong>("ids:owner")) return;

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id.ToString());

        if (user is null)
        {
            await dbContext.Users.AddAsync(new User()
            {
                Id = id.ToString(),
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

    public async Task<bool> SaveMuteAsync<T>(ulong id, DateTime createdAt, Func<T> factory) where T : Mute
    {
        var collection = dbContext.Set<T>();

        if (await collection.FirstOrDefaultAsync(u => u.Id == id.ToString()) != null) return false;

        var model = factory();
        model.ExpiresAt = createdAt.AddHours(config.GetValue<double>("timers:slowmodeHours"));
        model.CreatedAt = DateTime.Now;
        model.UpdatedAt = DateTime.Now;

        await collection.AddAsync(model);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<T>> GetAndDeleteExpiredMutesAsync<T>() where T : Mute
    {
        var collection = dbContext.Set<T>();
        var mutes = await collection.Where(x => x.ExpiresAt <= DateTime.Now).ToListAsync();

        if (mutes.Count > 0) collection.RemoveRange(mutes);
        await dbContext.SaveChangesAsync();

        return mutes;
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
