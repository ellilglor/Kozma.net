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

    private readonly MemoryCache _cache;
    private readonly UnboxTracker _unboxTracker;

    public UnboxTrackerTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _unboxTracker = new UnboxTracker(_cache);
    }

    private void SetPlayer()
    {
        _unboxTracker.SetPlayer(_ID, _BOX);
    }

    [Fact]
    public void SetPlayer_CreatesEmptyListInCache()
    {
        SetPlayer();
        Assert.Empty(_cache.Get<List<TrackerItem>>(_cacheKey)!);
    }

    [Fact]
    public void AddEntry_CreatesNewEntryWithValue1()
    {
        var expected = "Ultron";

        SetPlayer();
        _unboxTracker.AddEntry(_ID, _BOX, expected);

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
        _unboxTracker.AddEntry(_ID, _BOX, value);
        _unboxTracker.AddEntry(_ID, _BOX, value);

        Assert.Equal(2, _cache.Get<List<TrackerItem>>(_cacheKey)![0].Count);
    }

    [Fact]
    public void AddEntry_AddsMultipleEntriesWithoutOverriding()
    {
        var expected = "Loki";

        SetPlayer();
        _unboxTracker.AddEntry(_ID, _BOX, "Thanos");
        _unboxTracker.AddEntry(_ID, _BOX, expected);

        Assert.Equal(expected, _cache.Get<List<TrackerItem>>(_cacheKey)![1].Name);
    }

    [Fact]
    public void GetData_ReturnsStringEvenIfNoData()
    {
        Assert.IsType<string>(_unboxTracker.GetData(_ID, _BOX));
    }

    [Fact]
    public void GetData_ReturnsStringEvenIfEmptyList()
    {
        SetPlayer();

        Assert.IsType<string>(_unboxTracker.GetData(_ID, _BOX));
    }

    [Fact]
    public void GetData_StringContainsAddedItems()
    {
        var expected = "Ultron";

        SetPlayer();
        _unboxTracker.AddEntry(_ID, _BOX, expected);

        Assert.Contains(expected, _unboxTracker.GetData(_ID, _BOX), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetItemCount_Returns0IfNoData()
    {
        Assert.Equal(0, _unboxTracker.GetItemCount(_ID, _BOX));
    }

    [Fact]
    public void GetItemCount_Returns0IfEmptyList()
    {
        SetPlayer();

        Assert.Equal(0, _unboxTracker.GetItemCount(_ID, _BOX));
    }

    [Fact]
    public void GetItemCount_ReturnsCorrectCount()
    {
        SetPlayer();
        _unboxTracker.AddEntry(_ID, _BOX, "Thanos");
        _unboxTracker.AddEntry(_ID, _BOX, "Loki");

        Assert.Equal(2, _unboxTracker.GetItemCount(_ID, _BOX));
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
