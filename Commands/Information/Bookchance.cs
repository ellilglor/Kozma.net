using Discord;
using Discord.Interactions;
using Kozma.net.Factories;

namespace Kozma.net.Commands.Information;

public class Bookchance(IEmbedFactory embedFactory) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("bookchance", "Odds of getting at least 1 Book of Dark Rituals in x kills.")]
    public async Task ExecuteAsync(
        [Summary(description: "Amount of Black Kats you encountered"), MinValue(1)] int kats)
    {
        // can never truly be 100% so "round" to 99.99%
        var chance = kats < 2471 ? Math.Round((1 - Math.Pow((1 - 1.0 / 250), kats)) * 100, 2) : 99.99;
        var fields = new List<EmbedFieldBuilder>
        {
            embedFactory.CreateField("Black Kat spawn:", "1/90 or 1.11%"),
            embedFactory.CreateField("Book drop:", "1/250 or 0.4%"),
            embedFactory.CreateField("Overall chance per Kat:", "1/25000 or 0.004%"),
        };

        var embed = embedFactory.GetEmbed($"After killing {kats} Black {(kats > 1 ? "kats" : "kat")} you have a {chance}% chance of getting at least 1 Book of Dark Rituals.")
            .WithDescription("*Disclaimer: The chance to get a book stays the same for each kat, so killing 250 kats does not guarantee a drop.*")
            .WithThumbnailUrl("https://media3.spiralknights.com/wiki-images/9/91/Crafting-Book_of_Dark_Rituals.png")
            .WithFields(fields)
            .Build();

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed);
    }
}
