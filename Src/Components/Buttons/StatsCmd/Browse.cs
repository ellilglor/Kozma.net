using Discord.Interactions;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Components.Buttons.StatsCmd;

public class Browse(IStatPageTracker pageTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("stats-*")]
    public async Task ExecuteAsync(string action)
    {
        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = pageTracker.GetPage(Context.User.Id, action);
            msg.Components = pageTracker.GetComponents(Context.User.Id);
        });
    }
}
