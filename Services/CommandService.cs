using Kozma.net.Models;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Services;

public class CommandService(KozmaDbContext dbContext) : ICommandService
{
    public async Task<int> GetCommandUsageAsync(bool isGame)
    {
        var query = await dbContext.Commands
            .Where(cmd => cmd.IsGame == isGame)
            .ToListAsync();

        return query.Sum(cmd => cmd.Count);
    }

    public async Task<IEnumerable<CommandStats>> GetCommandsAsync(bool isGame, int total)
    {
        var query = await dbContext.Commands
            .Where(cmd => cmd.IsGame == isGame)
            .OrderByDescending(cmd => cmd.Count)
            .ThenBy(cmd => cmd.Name)
            .ToListAsync();

        return query.Select(cmd => new CommandStats(cmd, Math.Round((cmd.Count / (double)total) * 100, 2)));
    }
}
