using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Services;

public class PunchService(KozmaDbContext dbContext, IConfiguration config) : IPunchService
{
    public async Task UpdateOrSaveGamblerAsync(ulong id, string name, PunchPrices ticket)
    {
        if (id == config.GetValue<ulong>("ids:owner")) return;

        var user = await dbContext.Gamblers.FirstOrDefaultAsync(u => u.Id == id);
        var cost = (int)ticket;

        if (user is null)
        {
            await dbContext.Gamblers.AddAsync(new Gambler()
            {
                Id = id,
                Name = name,
                SingleTicket = ticket == PunchPrices.SingleTicket ? cost : 0,
                DoubleTicket = ticket == PunchPrices.DoubleTicket ? cost : 0,
                TripleTicket = ticket == PunchPrices.TripleTicket ? cost : 0,
                Total = cost
            });
        }
        else
        {
            user.Total += cost;
            if (ticket == PunchPrices.SingleTicket) user.SingleTicket += cost;
            else if (ticket == PunchPrices.DoubleTicket) user.DoubleTicket += cost;
            else user.TripleTicket += cost;

            if (user.Name != name) user.Name = name;

            dbContext.Gamblers.Update(user);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<long> GetTotalSpentAsync() =>
        await dbContext.Gamblers.SumAsync(g => (long)g.Total);

    public async Task<IEnumerable<DbStat>> GetGamblersAsync(int limit, long total)
    {
        var query = await dbContext.Gamblers
            .OrderByDescending(g => g.Total)
            .ThenBy(g => g.Name)
            .Take(limit)
            .ToListAsync();

        return query.Select(g => new DbStat(g.Name, g.Total, g.Total / (double)total));
    }
}
