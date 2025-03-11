using Discord.Interactions;
using Kozma.net.Src.Commands.Information;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Helpers;

namespace Kozma.net.Src.Components.Buttons.Pagination;

public class AbbreviationCmd(IDiscordPaginator paginator) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{ComponentIds.AbbreviationBase}*")]
    public async Task ExecuteAsync(string action)
    {
        var userKey = $"{CommandIds.Abbreviation}_{Context.User.Id}";

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = paginator.GetPage(Abbreviations.PagesCacheKey, userKey, action);
            msg.Components = paginator.GetComponents(Abbreviations.PagesCacheKey, userKey, ComponentIds.AbbreviationBase);
        });
    }
}
