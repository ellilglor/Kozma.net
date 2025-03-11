using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace Kozma.net.Src.Commands.Information;

public class Abbreviations(IEmbedHandler embedHandler, IFileReader jsonFileReader, IMemoryCache cache, IDiscordPaginator paginator) : InteractionModuleBase<SocketInteractionContext>
{
    public const string PagesCacheKey = "abbreviations_pages";
    private sealed record Abbreviation(List<string> Short, string Long);
    private sealed record AbbreviationSet(string Title, List<Abbreviation> Abbreviations);

    [SlashCommand(CommandIds.Abbreviation, "Gives a list of commonly used abbreviations.")]
    public async Task ExecuteAsync()
    {
        var userKey = $"{CommandIds.Abbreviation}_{Context.User.Id}";

        if (!cache.TryGetValue(PagesCacheKey, out List<Embed>? _))
        {
            var pages = await BuildPagesAsync();
            paginator.AddPageCounterAndSaveToCache(pages, PagesCacheKey);
        }

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = paginator.GetPage(PagesCacheKey, userKey, string.Empty);
            msg.Components = paginator.GetComponents(PagesCacheKey, userKey, ComponentIds.AbbreviationBase);
        });
    }

    private async Task<IList<EmbedBuilder>> BuildPagesAsync()
    {
        var info = await jsonFileReader.ReadAsync<IEnumerable<IEnumerable<AbbreviationSet>>>(Path.Combine("Data", "Abbreviations.json"));
        var pages = new List<EmbedBuilder>();

        foreach (var page in info)
        {
            var pageDesc = new StringBuilder();

            foreach (var section in page)
            {
                var title = Format.Header(section.Title, level: 3);
                var desc = string.Join("\n", section.Abbreviations.Select(a => $"{Format.Bold(string.Join(", ", a.Short.Select(s => s)))}: {a.Long}"));

                pageDesc.AppendLine(string.Join("\n", title, desc));
            }

            pages.Add(embedHandler.GetBasicEmbed(Format.Underline("Commonly used abbreviations")).WithDescription(pageDesc.ToString()));
        }

        return pages;
    }
}
