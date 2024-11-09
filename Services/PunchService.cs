﻿using Kozma.net.Models;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Services;

public class PunchService(KozmaDbContext dbContext) : IPunchService
{
    public async Task<long> GetTotalSpentAsync()
    {
        var query = await dbContext.Gamblers.ToListAsync();

        return query.Sum(g => (long)g.Total);
    }

    public async Task<IEnumerable<GamblerStats>> GetGamblersAsync(int limit, long total)
    {
        var query = await dbContext.Gamblers
            .OrderByDescending(g => g.Total)
            .ThenBy(g => g.Name)
            .Take(limit)
            .ToListAsync();

        return query.Select(g => new GamblerStats(g, Math.Round(g.Total / (double)total * 100, 2)));
    }
}
