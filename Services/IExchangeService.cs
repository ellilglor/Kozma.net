namespace Kozma.net.Services;

public interface IExchangeService
{
    int GetExchangeRate();
    void UpdateExchange(int rate);
}
