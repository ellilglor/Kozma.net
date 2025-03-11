using Discord;

namespace Kozma.net.Src.Helpers;

public interface IDiscordPaginator
{
    Embed GetPage(string pagesKey, string userKey, string action);
    MessageComponent GetComponents(string pagesKey, string userKey, string baseId);
    void AddPageCounterAndSaveToCache(IList<EmbedBuilder> pages, string pagesKey, bool addTitle = false);
}
