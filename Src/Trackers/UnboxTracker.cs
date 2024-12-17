using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace Kozma.net.Src.Trackers;

public class UnboxTracker(IMemoryCache cache) : IUnboxTracker
{
    private static readonly MemoryCacheEntryOptions _cacheOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };

    public void SetPlayer(ulong id, Box key) =>
        cache.Set(CreateCacheKey(id, key), new List<TrackerItem>(), _cacheOptions);

    public void AddEntry(ulong id, Box key, string value)
    {
        var cacheKey = CreateCacheKey(id, key);

        if (!cache.TryGetValue(cacheKey, out List<TrackerItem>? items) || items is null)
        {
            items = [new TrackerItem(value, 1)];
        }
        else
        {
            int index = items.FindIndex(i => i.Name == value);

            if (index == -1)
            {
                items.Add(new TrackerItem(value, 1));
            }
            else
            {
                items[index] = items[index] with { Count = items[index].Count + 1 };
            }
        }

        cache.Set(cacheKey, items, _cacheOptions);
    }

    public string GetData(ulong id, Box key)
    {
        if (!cache.TryGetValue(CreateCacheKey(id, key), out List<TrackerItem>? items) || items is null) return "This data no longer exists";

        var data = new StringBuilder();
        var unboxed = items.OrderBy(i => !i.Name.Contains('*', StringComparison.InvariantCulture)).ThenByDescending(i => i.Count);

        foreach (var item in unboxed)
        {
            if (data.Length + item.Name.Length >= ExtendedDiscordConfig.MaxEmbedDescChars - 50)
            {
                data.AppendLine("**I have reached the character limit!**");
                break;
            }

            data.AppendLine($"{item.Name}: {item.Count}");
        }

        return data.ToString();
    }

    public int GetItemCount(ulong id, Box key)
    {
        if (!cache.TryGetValue(CreateCacheKey(id, key), out List<TrackerItem>? items) || items is null)
        {
            return 0;
        }

        return items.Count;
    }

    private static string CreateCacheKey(ulong id, Box key) =>
        id + "_" + key;
}
