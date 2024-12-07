using Kozma.net.Src.Logging;
using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Kozma.net.Src.Services;

public class ExchangeService(KozmaDbContext dbContext, IMemoryCache cache, IBotLogger logger) : IExchangeService
{
    private const string _cacheKey = "Exchange_Rate";

    public async Task<int> GetExchangeRateAsync()
    {
        if (!cache.TryGetValue(_cacheKey, out int rate))
        {
            var exchange = await GetExchangeAsync();

            if (exchange != null)
            {
                rate = exchange.Rate;
                cache.Set(_cacheKey, rate, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            else
            {
                await LogNoRateAsync();
                return -1;
            }
        }
        
        return rate;
    }

    public async Task UpdateExchangeAsync(int rate)
    {
        var exchange = await GetExchangeAsync();

        if (exchange != null)
        {
            exchange.Rate = rate;

            cache.Set(_cacheKey, rate, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            dbContext.Exchange.Update(exchange);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            await LogNoRateAsync();
        }
    }

    private async Task<Exchange?> GetExchangeAsync() =>
        await dbContext.Exchange.FirstOrDefaultAsync(); // Should only have 1 entry so ID is not needed

    private async Task LogNoRateAsync() =>
        await logger.LogAsync("Failed to find exchange rate.", pingOwner: true);
}
