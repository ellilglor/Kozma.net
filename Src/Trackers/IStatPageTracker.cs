using Discord;

namespace Kozma.net.Src.Trackers;

public interface IStatPageTracker
{
    public Task BuildPagesAsync();
    public Embed GetPage(ulong id, string action = "");
    public MessageComponent GetComponents(ulong id);
}
