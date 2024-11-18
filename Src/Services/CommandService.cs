using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Kozma.net.Src.Services;

public class CommandService(KozmaDbContext dbContext) : ICommandService
{
    public async Task UpdateOrSaveCommandAsync(string name, bool isCommand = true)
    {
        var command = await dbContext.Commands.FirstOrDefaultAsync(cmd => name == cmd.Name);

        if (command is null)
        {
            await dbContext.Commands.AddAsync(new Command()
            {
                Id = ObjectId.GenerateNewId(),
                Name = name,
                Count = 1,
                IsGame = !isCommand
            });
        }
        else
        {
            command.Count++;
            dbContext.Commands.Update(command);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<int> GetCommandUsageAsync(bool isGame)
    {
        var query = await dbContext.Commands
            .Where(cmd => cmd.IsGame == isGame)
            .ToListAsync();

        return query.Sum(cmd => cmd.Count);
    }

    public async Task<IEnumerable<DbStat>> GetCommandsAsync(bool isGame, int total)
    {
        var query = await dbContext.Commands
            .Where(cmd => cmd.IsGame == isGame)
            .OrderByDescending(cmd => cmd.Count)
            .ThenBy(cmd => cmd.Name)
            .ToListAsync();

        return query.Select(cmd => new DbStat(cmd.Name, cmd.Count, cmd.Count / (double)total));
    }
}
