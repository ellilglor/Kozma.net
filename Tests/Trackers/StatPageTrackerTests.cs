using Discord;
using Kozma.net.Src;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Services;
using Kozma.net.Src.Trackers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace UnitTests.Trackers;

public class StatPageTrackerTests : IDisposable
{
    private const string _cacheKey = "stat_pages";

    private readonly StatPageTracker _tracker;
    private readonly MemoryCache _cache;
    private readonly Mock<IBot> _botMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IEmbedHandler> _embedHandlerMock;
    private readonly Mock<ICostCalculator> _costCalculatorMock;
    private readonly Mock<ICommandService> _commandServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IUnboxService> _unboxServiceMock;
    private readonly Mock<IPunchService> _punchServiceMock;
    private readonly Mock<ITradeLogService> _tradeLogServiceMock;

    public StatPageTrackerTests()
    {

        _botMock = new Mock<IBot>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _configMock = new Mock<IConfiguration>();
        _embedHandlerMock = new Mock<IEmbedHandler>();
        _costCalculatorMock = new Mock<ICostCalculator>();
        _commandServiceMock = new Mock<ICommandService>();
        _userServiceMock = new Mock<IUserService>();
        _unboxServiceMock = new Mock<IUnboxService>();
        _punchServiceMock = new Mock<IPunchService>();
        _tradeLogServiceMock = new Mock<ITradeLogService>();

        /*var configurationMock = new Mock<IConfiguration>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s.Value).Returns("123");
        configurationMock.Setup(x => x.GetSection("ids:server")).Returns(sectionMock.Object);

        _configMock = configurationMock;*/

        _tracker = new StatPageTracker(
            _botMock.Object,
            _configMock.Object,
            _cache,
            _embedHandlerMock.Object,
            _costCalculatorMock.Object,
            _commandServiceMock.Object,
            _userServiceMock.Object,
            _unboxServiceMock.Object,
            _punchServiceMock.Object,
            _tradeLogServiceMock.Object);
    }

    [Fact]
    public async Task BuildPagesAsync_WhenCacheHasValue_ShouldReturnImmediately()
    {
        _cache.Set(_cacheKey, new List<Embed>());

        await _tracker.BuildPagesAsync();

        _userServiceMock.Verify(x => x.GetTotalUsersCountAsync(), Times.Never);
    }

    /*[Fact]
    public async Task BuildPagesAsync_ShouldCacheResult()
    {
        _commandServiceMock.SetupSequence(cs => cs.GetCommandUsageAsync(false)).ReturnsAsync(100);
        _commandServiceMock.SetupSequence(cs => cs.GetCommandUsageAsync(true)).ReturnsAsync(200);
        _userServiceMock.Setup(us => us.GetTotalUsersCountAsync()).ReturnsAsync(500);
        _unboxServiceMock.Setup(us => us.GetBoxOpenedCountAsync()).ReturnsAsync(300);
        _tradeLogServiceMock.Setup(tls => tls.GetTotalLogCountAsync()).ReturnsAsync(400);
        _tradeLogServiceMock.Setup(tls => tls.GetTotalSearchCountAsync()).ReturnsAsync(600);

        var clientMock = new Mock<DiscordSocketClient>();
        clientMock.Setup(c => c.Guilds).Returns(new List<SocketGuild>());
        //clientMock.Setup(c => c.CurrentUser).Returns(new Mock<SocketSelfUser>().Object);
        clientMock.Setup(c => c.CurrentUser).Returns(
    new Mock<SocketSelfUser>(new object[] { null, null }) { CallBase = true }.Object
);
        _botMock.Setup(b => b.Client).Returns(clientMock.Object);

        _embedHandlerMock
            .Setup(eh => eh.GetBasicEmbed(It.IsAny<string>()))
            .Returns<string>(title =>
            {
                return new EmbedBuilder()
                    .WithTitle(title)
                    .WithColor(Color.Teal);
            });

        _embedHandlerMock
            .Setup(eh => eh.CreateField(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns<string, string, bool>((name, _, _) =>
            {
                return new EmbedFieldBuilder()
                    .WithName(name)
                    .WithValue("test");
            });

        _tradeLogServiceMock
            .Setup(x => x.CountOccurencesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>()))
            .ReturnsAsync((
                new[]
                {
                    new KeyValuePair<string, int>("term1", 5),
                    new KeyValuePair<string, int>("term2", 3),
                    new KeyValuePair<string, int>("term3", 1)
                }.OrderByDescending(x => x.Value),
                9
            ));

        await _tracker.BuildPagesAsync();

        Assert.NotNull(_cache.Get(_cacheKey));
    }*/

    [Fact]
    public void GetPage_ReturnsEmbedIfNoPages()
    {
        _embedHandlerMock.Setup(x => x.GetAndBuildEmbed(It.IsAny<string>())).Returns(new EmbedBuilder().Build());

        Assert.IsType<Embed>(_tracker.GetPage(0));
    }

    [Theory]
    [InlineData(2, 0, ComponentIds.StatsFirst)]
    [InlineData(1, 0, ComponentIds.StatsPrev)]
    [InlineData(0, 1, ComponentIds.StatsNext)]
    [InlineData(0, 2, ComponentIds.StatsLast)]
    [InlineData(int.MaxValue, 0, "Invalid Action")]
    public void GetPage_SetsCurrentPageAsExpected(int startPage, int expectedPage, string action)
    {
        var pageCacheKey = 123UL;
        _cache.Set(pageCacheKey, startPage);
        _cache.Set(_cacheKey, new List<Embed>
        {
            new EmbedBuilder().Build(), new EmbedBuilder().Build(), new EmbedBuilder().Build()
        });

        _tracker.GetPage(pageCacheKey, action);

        Assert.Equal(expectedPage, _cache.Get<int>(pageCacheKey));
    }

    [Fact]
    public void GetComponents_ReturnsComponentsIfCacheIsEmpty()
    {
        Assert.IsType<MessageComponent>(_tracker.GetComponents(0));
    }

    [Fact]
    public void GetComponents_ReturnsComponentsIfPagesExist()
    {
        _cache.Set(_cacheKey, new List<Embed>());
        _cache.Set(0, 0);

        Assert.IsType<MessageComponent>(_tracker.GetComponents(0));
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
