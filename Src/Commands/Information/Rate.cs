﻿using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Commands.Information;

public class Rate(IEmbedHandler embedHandler, IExchangeService exchangeService, IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand(CommandIds.Rate, "View the current crowns per energy rate used for /convert.")]
    public async Task ExecuteAsync(
        [Summary(name: "value", description: "Update the used exchange rate.")] int? newRate = null)
    {
        var embed = embedHandler.GetEmbed("You don't have permission to set a new rate!")
            .WithDescription($"I use this rate for calculating {Format.Code("/convert")}.");

        if (newRate != null)
        {
            var user = Context.Guild?.GetUser(Context.User.Id);

            if (user != null && user.Roles.Any(r => r.Id == config.GetValue<ulong>("ids:roles:admin") || r.Id == config.GetValue<ulong>("ids:roles:mod")))
            {
                await exchangeService.UpdateExchangeAsync(newRate.Value);
                embed.WithTitle($"The conversion rate has been changed to: {newRate}.");
            }
            else
            {
                embed.WithColor(Colors.Error);
            }
        }
        else
        {
            var rate = await exchangeService.GetExchangeRateAsync();

            embed.WithTitle(rate == -1 ? "Something went wrong while fetching the data." : $"The current crowns per energy rate is: {rate}.");
        }

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
    }
}
