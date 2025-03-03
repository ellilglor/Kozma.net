using Kozma.net.Src.Enums;
using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Kozma.net.Src.Services;

public class UnboxService(KozmaDbContext dbContext) : IUnboxService
{
    public async Task UpdateOrSaveBoxAsync(Box box)
    {
        var model = await dbContext.Boxes.FirstOrDefaultAsync(b => b.Name == box.ToString());

        if (model is null)
        {
            await dbContext.Boxes.AddAsync(new Unbox()
            {
                Id = ObjectId.GenerateNewId(),
                Name = box.ToString(),
                Count = 1
            });
        }
        else
        {
            model.Count++;
            dbContext.Boxes.Update(model);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<int> GetBoxOpenedCountAsync() =>
        await dbContext.Boxes.SumAsync(box => box.Count);

    public async Task<IEnumerable<UnboxStat>> GetBoxesAsync(int total)
    {
        var query = await dbContext.Boxes
            .OrderByDescending(box => box.Count)
            .ThenBy(box => box.Name)
            .ToListAsync();

        return query.Select(box => new UnboxStat(Enum.Parse<Box>(box.Name), box.Count, box.Count / (double)total));
    }
}
