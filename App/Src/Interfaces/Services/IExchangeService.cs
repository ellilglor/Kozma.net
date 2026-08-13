namespace Kozma.net.Src.Interfaces.Services;

public interface IExchangeService
{
    Task<int> GetExchangeRateAsync();
    Task UpdateExchangeAsync(int rate);
}
