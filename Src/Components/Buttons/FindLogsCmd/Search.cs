using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Commands.Information;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Components.Buttons.FindLogsCmd;

public class Search(IEmbedHandler embedHandler, ITradeLogService tradeLogService, IContentHelper contentHelper, IFileReader jsonFileReader, IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("research-*")]
    public async Task ExecuteAsync(string variantSearch)
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var command = new FindLogs(embedHandler, tradeLogService, contentHelper, jsonFileReader, config);
        var item = string.Join(" ", context.Message.Embeds.First().Title.Split(' ').Skip(5)).Replace("_", string.Empty);

        await ModifyOriginalResponseAsync(msg => msg.Components = new ComponentBuilder().Build());
        await command.SearchLogsAsync(contentHelper.FilterContent(item), item, months: 120, checkVariants: variantSearch.Equals("var"), checkClean: false, checkMixed: true, user: context.User);
    }
}
