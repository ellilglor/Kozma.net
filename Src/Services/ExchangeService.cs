using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Src.Services;

public class ExchangeService(KozmaDbContext dbContext) : IExchangeService
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
            // TODO: @me in logchannel
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
            // TODO: @me in logchannel
        }
    }

    private async Task<Exchange?> GetExchangeAsync()
    {
        // Should only have 1 entry so ID is not needed
        return await dbContext.Exchange.FirstOrDefaultAsync();
    }
}
