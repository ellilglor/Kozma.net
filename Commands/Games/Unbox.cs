using Discord;
using Discord.Interactions;
using Kozma.net.Enums;
using Kozma.net.Factories;
using Kozma.net.Helpers;
using Kozma.net.Models;

namespace Kozma.net.Commands.Games;

public class Unbox(IEmbedFactory embedFactory, IboxHelper boxHelper) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("unbox", "Simulate opening a Prize Box or Lockbox.")]
    public async Task ExecuteAsync(
        [Summary(name: "box", description: "Select the box you want to open.")] Box box)
    {
        await UnboxAsync(Context, box);
    }

    public async Task UnboxAsync(SocketInteractionContext context, Box box, int opened = 1)
    {
        var boxData = boxHelper.GetBox(box)!;
        var author = new EmbedAuthorBuilder().WithName(box.ToString()).WithIconUrl(boxData.Url);
        var cost = CalculateCost(opened, boxData);
        var fields = new List<EmbedFieldBuilder>
        {
            embedFactory.CreateField("Opened:", opened.ToString()),
            embedFactory.CreateField("Spent", boxData.Currency.Equals(BoxCurrency.Dollar) ? $"${cost:N2}" : $"{cost:N0} Energy"),
        };
        var embed = embedFactory.GetEmbed("You unboxed:").WithAuthor(author).WithFields(fields);
        var unboxed = await OpenAsync(box);
        
        if (unboxed.Count == 0)
        {
            await context.Interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithDescription("Something went wrong while trying to open the box.").Build());
            // TODO: log in case this happens?
            return;
        }

        await SendOpeningAnimationAsync(context, author, boxData.Gif);

        var url = unboxed.First().Url;
        var description = string.Join(" & ", unboxed.Select(item => item.Name));
        var components = new ComponentBuilder()
            .WithButton(emote: new Emoji("\U0001F501"), customId: "unbox-again", style: ButtonStyle.Secondary)
            .WithButton(emote: new Emoji("\U0001F4D8"), customId: "unbox-stats", style: ButtonStyle.Secondary);
        if (opened == 69) components.WithButton(emote: new Emoji("\U0001F4B0"), url: "https://www.gamblersanonymous.org/ga/", style: ButtonStyle.Link);

        await context.Interaction.ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed.WithDescription($"*{description}*").WithImageUrl(url).Build();
            msg.Components = components.Build();
        });
    }

    private async Task SendOpeningAnimationAsync(SocketInteractionContext context, EmbedAuthorBuilder author, string url)
    {
        var embed = embedFactory.GetEmbed(string.Empty)
            .WithAuthor(author)
            .WithImageUrl(url)
            .Build();

        await context.Interaction.ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = new ComponentBuilder().Build();
        });
        await Task.Delay(3000); // Give the gif time to play
    }

    private static double CalculateCost(int amount, BoxData box)
    {
        switch (box.Currency)
        {
            case BoxCurrency.Energy: return amount * box.Price;
            case BoxCurrency.Dollar:
                var cost = 0.00;

                var amount14Batches = amount / 14;
                cost += amount14Batches * 49.95;

                var amount5Batches = (amount - 14 * amount14Batches) / 5;
                cost += amount5Batches * 19.95;

                var extra = amount - 14 * amount14Batches - 5 * amount5Batches;
                cost += extra * 4.95;

                return Math.Round(cost, 2);
            default: return box.Price;
        }
    }

    private async Task<List<ItemData>> OpenAsync(Box box)
    {
        var items = await boxHelper.GetItemDataAsync(box);
        var bonusBoxes = new List<Box>() { Box.Confection, Box.Lucky };
        var unboxed = new List<ItemData>();
        var prevOdds = 0.00;
        var roll = new Random().NextDouble() * 100;

        foreach (var item in items)
        {
            if (prevOdds <= roll && roll < prevOdds + item.Chance)
            {
                unboxed.Add(item);

                /*if (bonusBoxes.Contains(box))
                {
                    var bonusItem = BonusRoll(box, content, roll, item.Name);
                    if (bonusItem != null)
                    {
                        unboxed.Add(bonusItem);
                    }
                }*/

                return unboxed;
            }

            prevOdds += item.Chance;
        }

        return unboxed;
    }
}
