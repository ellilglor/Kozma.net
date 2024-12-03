using Discord;

namespace Kozma.net.Src.Trackers;

public interface IStatPageTracker
{
    Task BuildPagesAsync();
    Embed GetPage(ulong id, string action = "");
    MessageComponent GetComponents(ulong id);
}
