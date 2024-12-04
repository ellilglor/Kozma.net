using Kozma.net.Src.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace Kozma.net.Src.Trackers;

public class PunchTracker(IMemoryCache cache) : IPunchTracker
{
    private static readonly MemoryCacheEntryOptions _cacheOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };
    private const string _types = "Types";
    private const string _grades = "Grades";

    public void SetPlayer(ulong id, string key)
    {
        var empty = new Dictionary<string, List<TrackerItem>>()
        {
            { _types, [] },
            { _grades, [] }
        };

        cache.Set(CreateCacheKey(id, key), empty, _cacheOptions);
    }

    public void AddEntry(ulong id, string key, string type, string grade)
    {
        var cacheKey = CreateCacheKey(id, key);

        if (!cache.TryGetValue(cacheKey, out Dictionary<string, List<TrackerItem>>? uvs) || uvs is null)
        {
            uvs = new Dictionary<string, List<TrackerItem>>()
            {
                { _types, [new TrackerItem(type, 1)] },
                { _grades, [new TrackerItem(grade, 1)] }
            };
        }
        else
        {
            var typeIndex = uvs[_types].FindIndex(t => t.Name == type);
            var gradeIndex = uvs[_grades].FindIndex(g => g.Name == grade);

            if (typeIndex == -1)
            {
                uvs[_types].Add(new TrackerItem(type, 1));
            }
            else
            {
                uvs[_types][typeIndex] = uvs[_types][typeIndex] with { Count = uvs[_types][typeIndex].Count + 1 };
            }

            if (gradeIndex == -1)
            {
                uvs[_grades].Add(new TrackerItem(grade, 1));
            }
            else
            {
                uvs[_grades][gradeIndex] = uvs[_grades][gradeIndex] with { Count = uvs[_grades][gradeIndex].Count + 1 };
            }
        }

        cache.Set(cacheKey, uvs, _cacheOptions);
    }

    public string GetData(ulong id, string key)
    {
        if (!cache.TryGetValue(CreateCacheKey(id, key), out Dictionary<string, List<TrackerItem>>? uvs) || uvs is null)
        {
            return "This data no longer exists";
        }

        var data = new StringBuilder("**In this session you rolled:**\n");
        var types = uvs[_types].OrderByDescending(i => i.Count);
        var grades = uvs[_grades].OrderByDescending(i => i.Count);

        data.AppendJoin("\n", types.Select(t => $"{t.Name}: {t.Count}"));
        data.AppendLine("\n\n**And got these grades:**");
        data.AppendJoin("\n", grades.Select(g => $"{g.Name}: {g.Count}"));

        return data.ToString();
    }

    private static string CreateCacheKey(ulong id, string key) =>
        id + "_" + key;
}
