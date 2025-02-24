using Discord;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace UnitTests.Helpers;

public class UpdateHelperTests
{
    private readonly UpdateHelper _helper;
    private readonly MemoryCache _cache;
    private readonly Mock<ITradeLogService> _tradeLogServiceMock;

    public UpdateHelperTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _tradeLogServiceMock = new Mock<ITradeLogService>();
        _helper = new UpdateHelper(_cache, _tradeLogServiceMock.Object);
    }

    [Fact]
    public void GetChannels_ReturnsDictionaryOfChannels()
    {
        var result = _helper.GetChannels();

        Assert.IsType<IReadOnlyDictionary<string, ulong>>(result, exactMatch: false);
    }

    [Fact]
    public async Task GetLogsAsync_ReturnsEmptyListIfNoNewLogs()
    {
        var mockMessage = new Mock<IMessage>();
        mockMessage.Setup(x => x.Content).Returns("Hi");

        var mockChannel = new Mock<IMessageChannel>();
        mockChannel.Setup(x => x.GetMessagesAsync(It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(new List<List<IMessage>>() { new() { mockMessage.Object } }.ToAsyncEnumerable());

        _tradeLogServiceMock.Setup(x => x.CheckIfLogExistsAsync(It.IsAny<ulong>()))
            .ReturnsAsync(true);

        var result = await _helper.GetLogsAsync(mockChannel.Object, limit: 1);

        Assert.Empty(result);
    }

    // => message.GetUmpUrl() is an extension method so can't be mocked
    /*[Fact]
    public async Task GetLogsAsync_ReturnsListIfNewLogs()
    {
        var authorMock = new Mock<IUser>();
        authorMock.Setup(x => x.Username).Returns("User");

        var mockMessage = new Mock<IMessage>();
        mockMessage.Setup(x => x.Content).Returns("Hi");
        mockMessage.Setup(x => x.Attachments).Returns([]);
        mockMessage.Setup(x => x.Author).Returns(authorMock.Object);
        mockMessage.Setup(x => x.GetJumpUrl()).Returns("url");

        var mockChannel = new Mock<IMessageChannel>();
        mockChannel.Setup(x => x.GetMessagesAsync(It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(new List<List<IMessage>>() { new() { mockMessage.Object } }.ToAsyncEnumerable());
        mockChannel.Setup(x => x.Name).Returns("name");

        _tradeLogServiceMock.Setup(x => x.CheckIfLogExistsAsync(It.IsAny<ulong>()))
            .ReturnsAsync(false);

        var result = await _helper.GetLogsAsync(mockChannel.Object, limit: 1);

        Assert.Single(result);
    }*/

    [Fact]
    public void ClearFindLogsCache_ClearsKeyListFromCache()
    {
        _cache.Set(CommandIds.FindLogs, new List<string> { "key" });

        _helper.ClearFindLogsCache();

        Assert.Empty((List<string>)_cache.Get(CommandIds.FindLogs)!);
    }

    [Fact]
    public void ClearFindLogsCache_ClearsCache()
    {
        var key = "key";
        _cache.Set(key, "item");
        _cache.Set(CommandIds.FindLogs, new List<string> { key });

        _helper.ClearFindLogsCache();

        Assert.Null(_cache.Get(key));
    }
}
