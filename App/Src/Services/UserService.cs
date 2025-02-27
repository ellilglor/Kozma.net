using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;

namespace Kozma.net.Src.Services;

public class UserService(KozmaDbContext dbContext, IConfiguration config) : IUserService
{
    public async Task UpdateOrSaveUserAsync(ulong id, string name, bool isCommand, string command)
    {
        if (id == config.GetValue<ulong>("ids:owner")) return;

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

    public async Task<bool> SaveMuteAsync(ulong id, string name, bool isWtb, DateTime msgCreatedAt)
    {
        if (await dbContext.TradeMutes.FirstOrDefaultAsync(u => u.UserId == id && u.IsWtb == isWtb) != null) return false;

        await dbContext.TradeMutes.AddAsync(new Mute()
        {
            Id = ObjectId.GenerateNewId(),
            Name = name,
            UserId = id,
            IsWtb = isWtb,
            CreatedAt = DateTime.Now,
            ExpiresAt = msgCreatedAt.AddHours(config.GetValue<double>("timers:slowmodeHours"))
        });

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Mute>> GetAndDeleteExpiredMutesAsync()
    {
        var mutes = await dbContext.TradeMutes.Where(x => x.ExpiresAt <= DateTime.Now).ToListAsync();

        if (mutes.Count > 0)
        {
            dbContext.TradeMutes.RemoveRange(mutes);
            await dbContext.SaveChangesAsync();
        }

        return mutes;
    }

    public async Task<IEnumerable<Mute>> GetMutesAsync() =>
        await dbContext.TradeMutes.ToListAsync();

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
