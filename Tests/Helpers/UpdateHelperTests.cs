using Discord;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Services;
using Moq;

namespace UnitTests.Helpers;

public class UpdateHelperTests
{
    private readonly UpdateHelper _helper;
    private readonly Mock<ITradeLogService> _tradeLogServiceMock;

    public UpdateHelperTests()
    {
        _tradeLogServiceMock = new Mock<ITradeLogService>();
        _helper = new UpdateHelper(_tradeLogServiceMock.Object);
    }

    [Fact]
    public void GetChannels_ReturnsDictionaryOfChannels()
    {
        Assert.IsType<IReadOnlyDictionary<string, ulong>>(_helper.GetChannels(), exactMatch: false);
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
}
