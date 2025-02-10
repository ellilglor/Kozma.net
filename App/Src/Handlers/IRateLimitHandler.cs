namespace Kozma.net.Src.Handlers;

public interface IRateLimitHandler
{
    void SetRateLimit(string msg);
    bool IsRateLimited();
}
