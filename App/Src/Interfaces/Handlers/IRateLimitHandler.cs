namespace Kozma.net.Src.Interfaces.Handlers;

public interface IRateLimitHandler
{
    void SetRateLimit(string msg);
    bool IsRateLimited();
}
