using Discord;
using Kozma.net.Src;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace UnitTests.Helpers;

public class DiscordPaginatorTests : IDisposable
{
    private readonly DiscordPaginator _paginator;
    private readonly MemoryCache _cache;
    private readonly Mock<IBot> _botMock;
    private readonly Mock<IEmbedHandler> _embedHandlerMock;

    public DiscordPaginatorTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _botMock = new Mock<IBot>();
        _embedHandlerMock = new Mock<IEmbedHandler>();
        _paginator = new DiscordPaginator(_botMock.Object, _cache, _embedHandlerMock.Object);
    }

    [Fact]
    public void GetPage_ReturnsEmbedIfNoPages()
    {
        _embedHandlerMock.Setup(x => x.GetAndBuildEmbed(It.IsAny<string>())).Returns(new EmbedBuilder().Build());

        var result = _paginator.GetPage(string.Empty, string.Empty, string.Empty);

        Assert.IsType<Embed>(result);
    }

    [Theory]
    [InlineData(2, 0, ComponentIds.First)]
    [InlineData(1, 0, ComponentIds.Previous)]
    [InlineData(0, 1, ComponentIds.Next)]
    [InlineData(0, 2, ComponentIds.Last)]
    [InlineData(int.MaxValue, 0, "Invalid Action")]
    public void GetPage_SetsCurrentPageAsExpected(int startPage, int expectedPage, string action)
    {
        var pagesCacheKey = "A platypus";
        var userCacheKey = "Perry the platypus";
        _cache.Set(userCacheKey, startPage);
        _cache.Set(pagesCacheKey, new List<Embed>
        {
            new EmbedBuilder().Build(), new EmbedBuilder().Build(), new EmbedBuilder().Build()
        });

        _paginator.GetPage(pagesCacheKey, userCacheKey, action);

        var result = _cache.Get<int>(userCacheKey);
        Assert.Equal(expectedPage, result);
    }

    [Fact]
    public void GetComponents_ReturnsComponentsIfCacheIsEmpty()
    {
        var result = _paginator.GetComponents(string.Empty, string.Empty, string.Empty);

        Assert.IsType<MessageComponent>(result);
    }

    [Fact]
    public void GetComponents_ReturnsComponentsIfPagesExist()
    {
        var pagesCacheKey = "A platypus";
        var userCacheKey = "Perry the platypus";
        _cache.Set(pagesCacheKey, new List<Embed>());
        _cache.Set(userCacheKey, 0);

        var result = _paginator.GetComponents(pagesCacheKey, userCacheKey, string.Empty);

        Assert.IsType<MessageComponent>(result);
    }

    [Fact]
    public void AddPageCounterAndSaveToCache_SavesToCache()
    {
        var key = "test_key";

        _paginator.AddPageCounterAndSaveToCache([], key);

        var result = _cache.Get<List<Embed>>(key)!;
        Assert.Empty(result);
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
