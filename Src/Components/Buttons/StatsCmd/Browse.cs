using Discord.Interactions;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Components.Buttons.StatsCmd;

public class Browse(IStatPageTracker pageTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("stats-*")]
    public async Task ExecuteAsync(string action)
    {
        var embed = pageTracker.GetPage(Context.User.Id, action);
        var components = pageTracker.GetComponents(Context.User.Id);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed;
            msg.Components = components;
        });
    }
}
