using Kozma.net.Src.Logging;
using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Src.Services;

public class ExchangeService(KozmaDbContext dbContext, IBotLogger logger) : IExchangeService
{
    public async Task<int> GetExchangeRateAsync()
    {
        var exchange = await GetExchangeAsync();

        if (exchange != null)
        {
            return exchange.Rate;
        }
        else
        {
            await LogNoRateAsync();
            return -1;
        }

    }

    public async Task UpdateExchangeAsync(int rate)
    {
        var exchange = await GetExchangeAsync();

        if (exchange != null)
        {
            exchange.Rate = rate;

            dbContext.Exchange.Update(exchange);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            await LogNoRateAsync();
        }
    }

    private async Task<Exchange?> GetExchangeAsync()
    {
        return await dbContext.Exchange.FirstOrDefaultAsync(); // Should only have 1 entry so ID is not needed
    }

    private async Task LogNoRateAsync()
    {
        await logger.LogAsync("Failed to find exchange rate.", pingOwner: true);
    }
}
