namespace Kozma.net.Services;

public interface IExchangeService
{
    Task<int> GetExchangeRateAsync();
    Task UpdateExchangeAsync(int rate);
}
