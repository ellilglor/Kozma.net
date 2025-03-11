using Discord.Interactions;
using Kozma.net.Src.Commands.Server;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Helpers;

namespace Kozma.net.Src.Components.Buttons.Pagination;

public class StatsCmd(IDiscordPaginator paginator) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{ComponentIds.StatsBase}*")]
    public async Task ExecuteAsync(string action)
    {
        var userKey = $"{CommandIds.Stats}_{Context.User.Id}";

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = paginator.GetPage(Stats.PagesCacheKey, userKey, action);
            msg.Components = paginator.GetComponents(Stats.PagesCacheKey, userKey, ComponentIds.StatsBase);
        });
    }
}
