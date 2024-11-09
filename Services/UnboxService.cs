using Kozma.net.Models;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Services;

public class UnboxService(KozmaDbContext dbContext) : IUnboxService
{
    public async Task<int> GetBoxOpenedCountAsync()
    {
        var query = await dbContext.Boxes.ToListAsync();

        return query.Sum(box => box.Count);
    }

    public async Task<IEnumerable<BoxStats>> GetBoxesAsync(int total)
    {
        var query = await dbContext.Boxes
            .OrderByDescending(box => box.Count)
            .ThenBy(box => box.Name)
            .ToListAsync();

        return query.Select(box => new BoxStats(box, Math.Round(box.Count / (double)total * 100, 2)));
    }
}
