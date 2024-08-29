using Discord.Interactions;
using Kozma.net.Factories;
using Kozma.net.Helpers;
using Kozma.net.Services;

namespace Kozma.net.Commands.Information;

public class FindLogs(IEmbedFactory embedFactory, ITradeLogService tradeLogService, IContentHelper contentHelper) : InteractionModuleBase<SocketInteractionContext>
{
    // TODO? change choice options to bool
    [SlashCommand("findlogs", "Search the tradelog database for any item.")]
    public async Task ExecuteAsync(
        [Summary(description: "Item the bot should look for."), MinLength(3), MaxLength(69)] string item,
        [Summary(description: "How far back the bot should search. Default: 6 months."), MinValue(1), MaxValue(120)] int months = 6,
        [Summary(description: "Check for color variants / item family tree. Default: yes."), Choice("Yes", "variant-search"), Choice("No", "single-search")] string? variants = null,
        [Summary(description: "Filter out high value uvs. Default: no."), Choice("Yes", "clean-search"), Choice("No", "dirty-search")] string? clean = null,
        [Summary(description: "Check the mixed-trades channel. Default: yes."), Choice("Yes", "mixed-search"), Choice("No", "mixed-ignore")] string? mixed = null)
    {
        var checkVariants = !string.IsNullOrEmpty(variants) && variants == "variant-search";
        var checkClean = !string.IsNullOrEmpty(clean) && clean == "clean-search";
        var checkMixed = !string.IsNullOrEmpty(mixed) && mixed == "mixed-search";

        var embed = embedFactory.GetEmbed($"Searching for __{item}__, I will dm you what I can find.")
            .WithDescription("### Info & tips when searching:\n- **Slime boxes**:\ncombination followed by *slime lockbox*\nExample: QQQ Slime Lockbox\n" +
                "- **UV's**:\nuse asi / ctr + med / high / very high / max\n" +
                "The bot automatically swaps asi & ctr so you don't have to search twice.\nExample: Brandish ctr very high asi high\n" +
                "- **Equipment**:\nThe bot looks for the entire family tree of your item!\n" +
                "So when you lookup *brandish* it will also match on *Combuster* & *Acheron*\n" +
                "- **Color Themes**:\ncertain colors with (expected) similar value are grouped for more results." +
                " Some examples include *Divine* & *Volcanic*, tech colors, standard colors, etc.\n" +
                "- **Sprite pods**:\ntype out as seen in game\nExample: Drakon Pod (Divine)")
            .Build();

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed);
    }

    private async Task SearchLogsAsync(string item, int months, bool checkVariants, bool checkClean, bool checkMixed)
    {
        var unedited = item;
        var items = new List<string>() { item };
        var reverse = new List<string>();
        var stopHere = DateTime.Now;
    }
}
