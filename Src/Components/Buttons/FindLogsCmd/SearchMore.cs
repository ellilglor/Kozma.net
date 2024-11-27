﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Commands.Information;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Components.Buttons.FindLogsCmd;

public class SearchMore(IEmbedHandler embedHandler, ITradeLogService tradeLogService, IFileReader jsonFileReader, IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("research-*")]
    public async Task ExecuteAsync(string variantSearch)
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var command = new FindLogs(embedHandler, tradeLogService, jsonFileReader, config);
        var item = string.Join(" ", context.Message.Embeds.First().Title.Split(' ').Skip(5)).Replace("_", string.Empty, StringComparison.InvariantCulture);

        await ModifyOriginalResponseAsync(msg => msg.Components = new ComponentBuilder().Build());
        await command.SearchLogsAsync(item.CleanUp(), item, months: 120, checkVariants: variantSearch == "var", checkClean: false, checkMixed: true, user: context.User);
    }
}