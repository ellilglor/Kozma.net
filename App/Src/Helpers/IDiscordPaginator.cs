using Discord;
using Microsoft.Extensions.Caching.Memory;

namespace Kozma.net.Src.Helpers;

public interface IDiscordPaginator
{
    MemoryCacheEntryOptions CacheOptions { get; }
    Embed GetPage(string pagesKey, string userKey, string action);
    MessageComponent GetComponents(string pagesKey, string userKey, string baseId);
}
