namespace Kozma.net.Src.Services;

public interface IExchangeService
{
    Task<int> GetExchangeRateAsync();
    Task UpdateExchangeAsync(int rate);
}
