using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Commands.Information;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Components.Buttons.FindLogsCmd;

public class SearchMore(IMemoryCache cache, IEmbedHandler embedHandler, ITradeLogService tradeLogService, IFileReader jsonFileReader, IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{ComponentIds.FindLogsBase}*")]
    public async Task ExecuteAsync(string variantSearch)
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var command = new FindLogs(cache, embedHandler, tradeLogService, jsonFileReader, config);
        var original = string.Join(" ", context.Message.Embeds.First().Title.Split(' ').Skip(5)).Replace("_", string.Empty, StringComparison.InvariantCulture);
        var checkVariants = variantSearch == ComponentIds.FindLogsVar;
        var altered = original.CleanUp();
        var months = 120;

        await ModifyOriginalResponseAsync(msg => msg.Components = new ComponentBuilder().Build());
        var matches = await command.SearchLogsAsync(altered, months, checkVariants, checkClean: false, checkMixed: true);
        await command.SendMatchesAsync(context.User, matches, altered, original, months, checkVariants);
    }
}
