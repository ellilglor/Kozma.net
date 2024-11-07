using Discord;

namespace Kozma.net.Trackers;

public interface IStatPageTracker
{
    public Task BuildPagesAsync();
    public Embed GetPage();
}
