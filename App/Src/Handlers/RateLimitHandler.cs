using System.Text.RegularExpressions;

namespace Kozma.net.Src.Handlers;

public partial class RateLimitHandler : IRateLimitHandler
{
    private static readonly object _rateLimitLock = new();
    private static bool _isRateLimited;
    private static DateTime _rateLimitEnd;

    public void SetRateLimit(string msg)
    {
        lock (_rateLimitLock)
        {
            _isRateLimited = true;

            if (ExtractRemainingTime(msg, out var remainingTime))
            {
                _rateLimitEnd = DateTime.UtcNow.Add(remainingTime);
            }
        }
    }

    public bool IsRateLimited()
    {
        lock (_rateLimitLock)
        {
            if (_isRateLimited && DateTime.UtcNow >= _rateLimitEnd) _isRateLimited = false;

            return _isRateLimited;
        }
    }

    private static bool ExtractRemainingTime(string message, out TimeSpan remainingTime)
    {
        var match = RateLimitRegex().Match(message);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int seconds))
        {
            remainingTime = TimeSpan.FromSeconds(seconds);
            return true;
        }

        remainingTime = TimeSpan.Zero;
        return false;
    }

    [GeneratedRegex(@"Remaining: (\d+)s")]
    private static partial Regex RateLimitRegex();
}
