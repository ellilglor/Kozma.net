using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;
using Kozma.net.Src.Trackers;
using Microsoft.Extensions.Caching.Memory;

namespace UnitTests.Trackers;

public class UnboxTrackerTests : IDisposable
{
    private const ulong _ID = 123;
    private const Box _BOX = Box.Equinox;
    private static readonly string _cacheKey = $"{_ID}_{_BOX}";

    private readonly UnboxTracker _tracker;
    private readonly MemoryCache _cache;

    public UnboxTrackerTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _tracker = new UnboxTracker(_cache);
    }

    private void SetPlayer()
    {
        _tracker.SetPlayer(_ID, _BOX);
    }

    [Fact]
    public void SetPlayer_CreatesEmptyListInCache()
    {
        SetPlayer();

        var result = _cache.Get<List<TrackerItem>>(_cacheKey)!;
        Assert.Empty(result);
    }

    [Fact]
    public void AddEntry_CreatesNewEntryWithValue1()
    {
        var expected = "Ultron";
        SetPlayer();

        _tracker.AddEntry(_ID, _BOX, expected);

        var items = _cache.Get<List<TrackerItem>>(_cacheKey)!;
        Assert.NotEmpty(items);
        Assert.Equal(1, items[0].Count);
        Assert.Equal(expected, items[0].Name);
    }

    [Fact]
    public void AddEntry_UpdatesExistingEntryBy1()
    {
        var value = "Thanos";
        SetPlayer();

        _tracker.AddEntry(_ID, _BOX, value);
        _tracker.AddEntry(_ID, _BOX, value);

        var result = _cache.Get<List<TrackerItem>>(_cacheKey)!;
        Assert.Equal(2, result[0].Count);
    }

    [Fact]
    public void AddEntry_AddsMultipleEntriesWithoutOverriding()
    {
        var expected = "Loki";
        SetPlayer();

        _tracker.AddEntry(_ID, _BOX, "Thanos");
        _tracker.AddEntry(_ID, _BOX, expected);

        var result = _cache.Get<List<TrackerItem>>(_cacheKey)!;
        Assert.Equal(expected, result[1].Name);
    }

    [Fact]
    public void GetData_ReturnsStringEvenIfNoData()
    {
        var result = _tracker.GetData(_ID, _BOX);

        Assert.IsType<string>(result);
    }

    [Fact]
    public void GetData_ReturnsStringEvenIfEmptyList()
    {
        SetPlayer();

        var result = _tracker.GetData(_ID, _BOX);

        Assert.IsType<string>(result);
    }

    [Fact]
    public void GetData_StringContainsAddedItems()
    {
        var expected = "Ultron";
        SetPlayer();
        _tracker.AddEntry(_ID, _BOX, expected);

        var result = _tracker.GetData(_ID, _BOX);

        Assert.Contains(expected, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetItemCount_Returns0IfNoData()
    {
        var result = _tracker.GetItemCount(_ID, _BOX);

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetItemCount_Returns0IfEmptyList()
    {
        SetPlayer();

        var result = _tracker.GetItemCount(_ID, _BOX);

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetItemCount_ReturnsCorrectCount()
    {
        SetPlayer();
        _tracker.AddEntry(_ID, _BOX, "Thanos");
        _tracker.AddEntry(_ID, _BOX, "Loki");

        var result = _tracker.GetItemCount(_ID, _BOX);

        Assert.Equal(2, result);
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
