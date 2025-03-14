﻿using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Services;

namespace Kozma.net.Src.Commands.Information;

public class ConvertCurrency(IEmbedHandler embedHandler, IExchangeService exchangeService) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand(CommandIds.Convert, "Convert crowns or energy into the other currency.")]
    public async Task ExecuteAsync(
        [Summary(description: "Currency you want to convert.")] Currency currency,
        [Summary(description: "Amount you want to convert."), MinValue(1000)] int amount,
        [Summary(description: "Optional custom conversion rate."), MinValue(1)] int? rate = null)
    {
        var exchange = rate ?? await exchangeService.GetExchangeRateAsync();
        var converted = currency == Currency.energy ? amount * exchange : amount / exchange;

        var title = currency switch
        {
            Currency.crowns => $"{amount:N0} Crowns is equal to roughly {converted:N0} Energy.",
            Currency.energy => $"{amount:N0} Energy is equal to roughly {converted:N0} Crowns.",
            _ => throw new ArgumentException("The provided currency was invalid")
        };

        var embed = embedHandler.GetEmbed(title)
            .WithDescription($"Used conversion rate: {Format.Bold(exchange.ToString())} Crowns per Energy.");

        if (currency == Currency.crowns) embed.WithColor(Colors.Crown);

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
    }
}
