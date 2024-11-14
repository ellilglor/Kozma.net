using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Commands.Information;
using Kozma.net.Handlers;
using Kozma.net.Helpers;
using Kozma.net.Services;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Components.Buttons.FindLogsCmd;

public class Search(IEmbedHandler embedHandler, ITradeLogService tradeLogService, IContentHelper contentHelper, IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("research-*")]
    public async Task ExecuteAsync(string variantSearch)
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var command = new FindLogs(embedHandler, tradeLogService, contentHelper, config);
        var item = string.Join(" ", context.Message.Embeds.First().Title.Split(' ').Skip(5)).Replace("_", string.Empty);

        await ModifyOriginalResponseAsync(msg => msg.Components = new ComponentBuilder().Build());
        await command.SearchLogsAsync(item, months: 120, checkVariants: variantSearch.Equals("var"), checkClean: false, checkMixed: true, user: context.User);
    }
}
