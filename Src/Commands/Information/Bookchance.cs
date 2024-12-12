using Discord;
using Discord.Interactions;
using Kozma.net.Src.Handlers;

namespace Kozma.net.Src.Commands.Information;

public class Bookchance(IEmbedHandler embedHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("bookchance", "Odds of getting at least 1 Book of Dark Rituals in x kills.")]
    public async Task ExecuteAsync(
        [Summary(description: "Amount of Black Kats you encountered"), MinValue(1)] int kats)
    {
        // can never truly be 100% so "round" to 99.99%
        var chance = kats < 2471 ? 1 - Math.Pow(1 - 1.0 / 250, kats) : 0.9999;

        var fields = new List<EmbedFieldBuilder>
        {
            embedHandler.CreateField("Black Kat spawn:", "1/90 or 1.11%"),
            embedHandler.CreateField("Book drop:", "1/250 or 0.4%"),
            embedHandler.CreateField("Overall chance per Kat:", "1/25000 or 0.004%"),
        };

        var embed = embedHandler.GetEmbed($"After killing {kats} Black {(kats > 1 ? "kats" : "kat")} you have a {chance:P2} chance of getting at least 1 Book of Dark Rituals.")
            .WithDescription("*Disclaimer: The chance to get a book stays the same for each kat, so killing 250 kats does not guarantee a drop.*")
            .WithThumbnailUrl("https://media3.spiralknights.com/wiki-images/9/91/Crafting-Book_of_Dark_Rituals.png")
            .WithFields(fields);

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
    }
}
