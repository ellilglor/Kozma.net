using Discord.Interactions;
using Kozma.net.Enums;
using Kozma.net.Factories;
using Kozma.net.Services;

namespace Kozma.net.Commands.Information;

public class ConvertCurrency(IEmbedFactory embedFactory, IExchangeService exchangeService) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("convert", "Convert crowns or energy into the other currency.")]
    public async Task ExecuteAsync(
        [Summary(description: "Currency you want to convert.")] Currency currency,
        [Summary(description: "Amount you want to convert."), MinValue(1000)] int amount,
        [Summary(description: "Optional custom conversion rate."), MinValue(1)] int? rate = null)
    {
        var exchange = rate ?? exchangeService.GetExchangeRate();
        var converted = currency == Currency.energy ? (amount * exchange) : (amount / exchange);

        var title = currency switch
        {
            Currency.crowns => $"{amount:N0} Crowns is equal to roughly {converted:N0} Energy.",
            Currency.energy => $"{amount:N0} Energy is equal to roughly {converted:N0} Crowns.",
            _ => "The provided currency was invalid."
        };

        var embed = embedFactory.GetEmbed(title)
            .WithDescription($"Used conversion rate: **{exchange}** Crowns per Energy.");

        if (currency == Currency.crowns)
        {
            embed.WithColor(0xf9d49c);
        }

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
    }
}
