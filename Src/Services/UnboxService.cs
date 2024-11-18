using Kozma.net.Src.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Src.Services;

public class UnboxService(KozmaDbContext dbContext) : IUnboxService
{
    public async Task<int> GetBoxOpenedCountAsync()
    {
        var query = await dbContext.Boxes.ToListAsync();

        return query.Sum(box => box.Count);
    }

    public async Task<IEnumerable<UnboxStat>> GetBoxesAsync(int total)
    {
        var query = await dbContext.Boxes
            .OrderByDescending(box => box.Count)
            .ThenBy(box => box.Name)
            .ToListAsync();

        return query.Select(box => new UnboxStat(box.Name, box.Count, box.Count / (double)total));
    }
}
