using Discord.Interactions;
using Kozma.net.Enums;
using Kozma.net.Factories;
using Kozma.net.Services;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Commands.Information;

public class Rate(IEmbedFactory embedFactory, IExchangeService exchangeService, IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("rate", "View the current crowns per energy rate used for /convert.")]
    public async Task ExecuteAsync(
        [Summary(name:"value", description: "Update the used exchange rate.")] int? newRate = null)
    {
        var embed = embedFactory.GetEmbed("You don't have permission to set a new rate!")
            .WithDescription("I use this rate for calculating **/convert**.");

        if (newRate != null)
        {
            var user = Context.Guild?.GetUser(Context.User.Id);

            if (user != null && user.Roles.Any(r => r.Id == config.GetValue<ulong>("ids:adminId") || r.Id == config.GetValue<ulong>("ids:modId")))
            {
                exchangeService.UpdateExchange((int) newRate);
                embed.WithTitle($"The conversion rate has been changed to: {newRate}.");
            } else
            {
                embed.WithColor(embedFactory.ConvertEmbedColor(EmbedColor.Error));
            }
        }
        else
        {
            var rate = exchangeService.GetExchangeRate();

            embed.WithTitle(rate == -1 ? "Something went wrong while fetching the data." : $"The current crowns per energy rate is: {rate}.");
        }

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
    }
}
