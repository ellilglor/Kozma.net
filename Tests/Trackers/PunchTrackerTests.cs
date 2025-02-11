using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;
using Kozma.net.Src.Trackers;
using Microsoft.Extensions.Caching.Memory;

namespace UnitTests.Trackers;

public class PunchTrackerTests : IDisposable
{
    private const ulong _ID = 123;
    private const string _ITEM = "Antigua";
    private static readonly string _cacheKey = $"{_ID}_{_ITEM}";
    private const string _types = "Types";
    private const string _grades = "Grades";

    private readonly MemoryCache _cache;
    private readonly PunchTracker _punchTracker;

    public PunchTrackerTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _punchTracker = new PunchTracker(_cache);
    }

    private void SetPlayer()
    {
        _punchTracker.SetPlayer(_ID, _ITEM);
    }

    [Fact]
    public void SetPlayer_CreatesEmptyCollectionInCache()
    {
        SetPlayer();

        var cached = _cache.Get<Dictionary<string, List<TrackerItem>>>(_cacheKey)!;
        Assert.Empty(cached[_types]);
        Assert.Empty(cached[_grades]);
    }

    [Fact]
    public void AddEntry_CreatesNewEntryWithValue1()
    {
        var expectedType = "Susan Storm";
        var expectedGrade = "Johnny Storm";

        SetPlayer();
        _punchTracker.AddEntry(_ID, _ITEM, expectedType, expectedGrade);

        var cached = _cache.Get<Dictionary<string, List<TrackerItem>>>(_cacheKey)!;

        Assert.NotEmpty(cached[_types]);
        Assert.Equal(1, cached[_types][0].Count);
        Assert.Equal(expectedType, cached[_types][0].Name);

        Assert.NotEmpty(cached[_grades]);
        Assert.Equal(1, cached[_grades][0].Count);
        Assert.Equal(expectedGrade, cached[_grades][0].Name);
    }

    [Fact]
    public void AddEntry_UpdatesExistingEntryBy1()
    {
        var type = "Reed Richards";
        var grade = "Ben Grimm";

        SetPlayer();
        _punchTracker.AddEntry(_ID, _ITEM, type, grade);
        _punchTracker.AddEntry(_ID, _ITEM, type, grade);

        var cached = _cache.Get<Dictionary<string, List<TrackerItem>>>(_cacheKey)!;
        Assert.Equal(2, cached[_types][0].Count);
        Assert.Equal(2, cached[_grades][0].Count);
    }

    [Fact]
    public void AddEntry_AddsMultipleEntriesWithoutOverriding()
    {
        var expectedType = "Galactus";
        var expectedGrade = "Silver Surfer";

        SetPlayer();
        _punchTracker.AddEntry(_ID, _ITEM, "Reed Richards", "Susan Storm");
        _punchTracker.AddEntry(_ID, _ITEM, expectedType, expectedGrade);

        var cached = _cache.Get<Dictionary<string, List<TrackerItem>>>(_cacheKey)!;
        Assert.Equal(expectedType, cached[_types][1].Name);
        Assert.Equal(expectedGrade, cached[_grades][1].Name);
    }

    [Fact]
    public void GetData_ReturnsStringEvenIfNoData()
    {
        Assert.IsType<string>(_punchTracker.GetData(_ID, _ITEM));
    }

    [Fact]
    public void GetData_ReturnsStringEvenIfEmptyList()
    {
        SetPlayer();

        Assert.IsType<string>(_punchTracker.GetData(_ID, _ITEM));
    }

    [Fact]
    public void GetData_StringContainsAddedValues()
    {
        var expectedType = "Doctor";
        var expectedGrade = "Doom";

        SetPlayer();
        _punchTracker.AddEntry(_ID, _ITEM, expectedType, expectedGrade);

        var data = _punchTracker.GetData(_ID, _ITEM);
        Assert.Contains(expectedType, data, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(expectedGrade, data, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
