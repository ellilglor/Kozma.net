using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Components.Buttons.StatsCmd;

public class Browse(IStatPageTracker pageTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{ComponentIds.StatsBase}*")]
    public async Task ExecuteAsync(string action)
    {
        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = pageTracker.GetPage(Context.User.Id, action);
            msg.Components = pageTracker.GetComponents(Context.User.Id);
        });
    }
}
