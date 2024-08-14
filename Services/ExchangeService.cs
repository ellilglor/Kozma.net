using Kozma.net.Models;

namespace Kozma.net.Services;

public class ExchangeService(KozmaDbContext dbContext) : IExchangeService
{
    public int GetExchangeRate()
    {
        var exchange = GetExchange();

        if (exchange != null)
        {
            return exchange.Rate;
        } else
        {
            // TODO: @me in logchannel
            return -1;
        }

    }

    public void UpdateExchange(int rate)
    {
        var exchange = GetExchange();

        if (exchange != null)
        {
            exchange.Rate = rate;

            dbContext.Exchange.Update(exchange);
            dbContext.SaveChanges();
        } else
        {
            // TODO: @me in logchannel
        }
    }

    private Exchange? GetExchange()
    {
        // Should only have 1 entry so ID is not needed
        return dbContext.Exchange.FirstOrDefault();
    }
}
