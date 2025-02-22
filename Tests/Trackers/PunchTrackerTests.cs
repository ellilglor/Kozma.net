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

    private readonly PunchTracker _tracker;
    private readonly MemoryCache _cache;

    public PunchTrackerTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _tracker = new PunchTracker(_cache);
    }

    private void SetPlayer()
    {
        _tracker.SetPlayer(_ID, _ITEM);
    }

    [Fact]
    public void SetPlayer_CreatesEmptyCollectionInCache()
    {
        SetPlayer();

        var result = _cache.Get<Dictionary<string, List<TrackerItem>>>(_cacheKey)!;
        Assert.Empty(result[_types]);
        Assert.Empty(result[_grades]);
    }

    [Fact]
    public void AddEntry_CreatesNewEntryWithValue1()
    {
        var expectedType = "Susan Storm";
        var expectedGrade = "Johnny Storm";
        SetPlayer();

        _tracker.AddEntry(_ID, _ITEM, expectedType, expectedGrade);

        var result = _cache.Get<Dictionary<string, List<TrackerItem>>>(_cacheKey)!;

        Assert.NotEmpty(result[_types]);
        Assert.Equal(1, result[_types][0].Count);
        Assert.Equal(expectedType, result[_types][0].Name);

        Assert.NotEmpty(result[_grades]);
        Assert.Equal(1, result[_grades][0].Count);
        Assert.Equal(expectedGrade, result[_grades][0].Name);
    }

    [Fact]
    public void AddEntry_UpdatesExistingEntryBy1()
    {
        var type = "Reed Richards";
        var grade = "Ben Grimm";
        SetPlayer();

        _tracker.AddEntry(_ID, _ITEM, type, grade);
        _tracker.AddEntry(_ID, _ITEM, type, grade);

        var result = _cache.Get<Dictionary<string, List<TrackerItem>>>(_cacheKey)!;
        Assert.Equal(2, result[_types][0].Count);
        Assert.Equal(2, result[_grades][0].Count);
    }

    [Fact]
    public void AddEntry_AddsMultipleEntriesWithoutOverriding()
    {
        var expectedType = "Galactus";
        var expectedGrade = "Silver Surfer";
        SetPlayer();

        _tracker.AddEntry(_ID, _ITEM, "Reed Richards", "Susan Storm");
        _tracker.AddEntry(_ID, _ITEM, expectedType, expectedGrade);

        var result = _cache.Get<Dictionary<string, List<TrackerItem>>>(_cacheKey)!;
        Assert.Equal(expectedType, result[_types][1].Name);
        Assert.Equal(expectedGrade, result[_grades][1].Name);
    }

    [Fact]
    public void GetData_ReturnsStringEvenIfNoData()
    {
        var result = _tracker.GetData(_ID, _ITEM);

        Assert.IsType<string>(result);
    }

    [Fact]
    public void GetData_ReturnsStringEvenIfEmptyList()
    {
        SetPlayer();

        var result = _tracker.GetData(_ID, _ITEM);

        Assert.IsType<string>(result);
    }

    [Fact]
    public void GetData_StringContainsAddedValues()
    {
        var expectedType = "Doctor";
        var expectedGrade = "Doom";
        SetPlayer();
        _tracker.AddEntry(_ID, _ITEM, expectedType, expectedGrade);

        var result = _tracker.GetData(_ID, _ITEM);

        Assert.Contains(expectedType, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(expectedGrade, result, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
