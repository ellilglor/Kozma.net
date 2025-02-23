using Kozma.net.Src.Commands.Information;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Models.Entities;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace UnitTests.Commands;

public class FindLogsTests : IDisposable
{
    private readonly FindLogs _command;
    private readonly MemoryCache _cache;
    private readonly Mock<IEmbedHandler> _embedHandlerMock;
    private readonly Mock<ITradeLogService> _tradeLogServiceMock;
    private readonly Mock<IFileReader> _fileReaderMock;
    private readonly Mock<IConfiguration> _configMock;

    public FindLogsTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _embedHandlerMock = new Mock<IEmbedHandler>();
        _tradeLogServiceMock = new Mock<ITradeLogService>();
        _fileReaderMock = new Mock<IFileReader>();
        _configMock = new Mock<IConfiguration>();

        _command = new FindLogs(
            _cache,
            _embedHandlerMock.Object,
            _tradeLogServiceMock.Object,
            _fileReaderMock.Object,
            _configMock.Object);
    }

    [Fact]
    public async Task SearchLogsAsync_WhenCacheHasValue_ShouldReturnImmediately()
    {
        var template = "Ant Man";
        var months = 1;
        var checkX = true;
        var cacheKey = $"{template}_{months}_{checkX}_{checkX}_{checkX}";
        _cache.Set(cacheKey, new List<LogGroups>());

        await _command.SearchLogsAsync(template, months, checkX, checkX, checkX);

        _tradeLogServiceMock.Verify(x =>
            x.GetLogsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<string>>()), Times.Never);
    }

    [Fact]
    public async Task SearchLogsAsync_CanFindMatch()
    {
        var expectedLogs = new List<LogGroups> { new() {
            Channel = string.Empty,
            Messages = []
        } };

        _tradeLogServiceMock.Setup(x => x.GetLogsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<string>>()))
            .ReturnsAsync(expectedLogs);

        _fileReaderMock.Setup(x => x.ReadAsync<IEnumerable<string>>(It.IsAny<string>()))
            .ReturnsAsync([]);

        var result = await _command.SearchLogsAsync(string.Empty, 0, false, false, false);

        Assert.NotNull(result);
        Assert.Equal(expectedLogs, result);
    }

    [Fact]
    public async Task SearchLogsAsync_AddsKeyToCache()
    {
        var template = "Ant Man";
        var months = 1;
        var checkX = false;
        var cacheKey = $"{template}_{months}_{checkX}_{checkX}_{checkX}";

        _tradeLogServiceMock.Setup(x => x.GetLogsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IReadOnlyCollection<string>>()))
            .ReturnsAsync([]);

        _fileReaderMock.Setup(x => x.ReadAsync<IEnumerable<string>>(It.IsAny<string>()))
            .ReturnsAsync([]);

        await _command.SearchLogsAsync(template, months, checkX, checkX, checkX);

        var result = (List<string>)_cache.Get(CommandIds.FindLogs)!;

        Assert.NotEmpty(result);
        Assert.Equal(cacheKey, result[0]);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("brandish no uvs", "brandish no uvs")]
    [InlineData("ctr high brandish", "brandish ctr high")]
    [InlineData("asi very high brandish", "brandish asi very high")]
    [InlineData("shadow max black kat cowl", "black kat cowl shadow max")]
    [InlineData("normal low black kat cowl", "black kat cowl normal low")]
    public void AttachUvsToBack_ReturnsStringWithUvsAtTheEnd(string input, string expected)
    {
        var result = FindLogs.AttachUvsToBack(input);

        Assert.Equal(expected, result, ignoreCase: true);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("brandish no uvs", "brandish no uvs")]
    [InlineData("BRANDISH CTR LOW", "BRANDISH CTR LOW")]
    [InlineData("BRANDISH CTR HIGH ASI HIGH", "BRANDISH ASI HIGH CTR HIGH")]
    [InlineData("BRANDISH ASI VERY HIGH CTR VERY HIGH", "BRANDISH CTR VERY HIGH ASI VERY HIGH")]
    public void SwapUvs_ReturnsStringWithUvPositionSwapped(string input, string expected)
    {
        var result = FindLogs.SwapUvs(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("drakon")]
    [InlineData("maskeraith")]
    [InlineData("nog")]
    public async Task AddVariantsAsync_DoesNotModifyListIfException(string exception)
    {
        var input = new List<string>() { exception };
        var copy = input;

        await _command.AddVariantsAsync(input);

        Assert.Equal(copy, input);
    }

    [Fact]
    public async Task AddVariantsAsync_DoesNotModifyListIfNoMatches()
    {
        var input = new List<string>() { "The Winter Soldier" };
        var copy = input;

        _fileReaderMock.Setup(x => x.ReadAsync<IReadOnlyDictionary<string, List<string>>>(It.IsAny<string>()))
            .ReturnsAsync(new Dictionary<string, List<string>>());

        await _command.AddVariantsAsync(input);

        Assert.Equal(copy, input);
    }

    [Fact]
    public async Task AddVariantsAsync_Equipment_ModifiesList()
    {
        var input = new List<string>() { "Brandish" };
        var expected = new List<string>() { "Brandish", "Combuster", "Voltedge" };

        _fileReaderMock.Setup(x => x.ReadAsync<IReadOnlyDictionary<string, List<string>>>(It.Is<string>(x => x.Contains("EquipmentFamilies.json"))))
            .ReturnsAsync(new Dictionary<string, List<string>> { { "key", expected } });

        await _command.AddVariantsAsync(input);

        Assert.Equal(expected, input);
    }

    [Fact]
    public async Task AddVariantsAsync_Equipment_ModifiesListAndKeepsUvs()
    {
        var input = new List<string>() { "Brandish ctr med" };
        var expected = new List<string>() { "Brandish ctr med", "Combuster ctr med" };

        _fileReaderMock.Setup(x => x.ReadAsync<IReadOnlyDictionary<string, List<string>>>(It.Is<string>(x => x.Contains("EquipmentFamilies.json"))))
            .ReturnsAsync(new Dictionary<string, List<string>> { { "key", new List<string> { "Brandish", "Combuster" } } });

        await _command.AddVariantsAsync(input);

        Assert.Equal(expected, input);
    }

    [Fact]
    public async Task AddVariantsAsync_Equipment_RespectsExceptions()
    {
        var input = new List<string>() { "Frenzy Avenger Helm" };
        var expected = input;

        _fileReaderMock.Setup(x => x.ReadAsync<IReadOnlyDictionary<string, List<string>>>(It.Is<string>(x => x.Contains("EquipmentFamilies.json"))))
            .ReturnsAsync(new Dictionary<string, List<string>> { { "key", new List<string> { "AVENGER", "DIVINE AVENGER" } } });

        await _command.AddVariantsAsync(input);

        Assert.Equal(expected, input);
    }

    [Theory]
    [InlineData("key", "Cool Goggles",
        new string[] { "Cool Goggles", "Dusky Goggles" }, new string[] { "Cool", "Dusky" })]
    [InlineData("OBSIDIAN", "Mantle of Sight",
        new string[] { "Mantle of Sight", "Mantle of Rituals" }, new string[] { "Sight", "Rituals" })]
    [InlineData("GEMS", "Floating Diamonds",
        new string[] { "Floating Emeralds", "Floating Diamonds", }, new string[] { "Emeralds", "Diamonds" })]
    [InlineData("rose key", "Tabard of the Blue Rose",
        new string[] { "Tabard of the Red Rose", "Tabard of the Blue Rose", }, new string[] { "the Red Rose", "the Blue Rose" })]
    public async Task AddVariantsAsync_Color_ModifiesList(string key, string input, string[] expected, string[] file)
    {
        var inputList = new List<string> { input };
        var expectedList = expected.ToList();
        _fileReaderMock.Setup(x => x.ReadAsync<IReadOnlyDictionary<string, List<string>>>(It.Is<string>(x => x.Contains("EquipmentFamilies.json"))))
            .ReturnsAsync(new Dictionary<string, List<string>>());
        _fileReaderMock.Setup(x => x.ReadAsync<IReadOnlyDictionary<string, List<string>>>(It.Is<string>(x => x.Contains("Colors.json"))))
            .ReturnsAsync(new Dictionary<string, List<string>> { { key, file.ToList() } });

        await _command.AddVariantsAsync(inputList);

        Assert.Equal(expectedList, inputList);
    }

    [Theory]
    [InlineData("GEMS", "Tabard of the Blue Rose", new string[] { "the Red Rose", "the Blue Rose" })]
    [InlineData("SNIPES", "Slime Node Mask", new string[] { "Lime", "Mint" })]
    [InlineData("SNIPES", "Fancy Plume", new string[] { "Plum", "Cool" })]
    [InlineData("SNIPES", "Peppermint Repeater", new string[] { "Lime", "Mint" })]
    [InlineData("key", "Tabard of the Blue Rose", new string[] { "ROSE", "Mint" })]
    [InlineData("key", "Chapeau of the Blue Rose", new string[] { "ROSE", "Mint" })]
    public async Task AddVariantsAsync_Color_RespectsExceptions(string key, string input, string[] file)
    {
        var inputList = new List<string> { input };
        var expected = inputList;
        _fileReaderMock.Setup(x => x.ReadAsync<IReadOnlyDictionary<string, List<string>>>(It.Is<string>(x => x.Contains("EquipmentFamilies.json"))))
            .ReturnsAsync(new Dictionary<string, List<string>>());
        _fileReaderMock.Setup(x => x.ReadAsync<IReadOnlyDictionary<string, List<string>>>(It.Is<string>(x => x.Contains("Colors.json"))))
            .ReturnsAsync(new Dictionary<string, List<string>> { { key, file.ToList() } });

        await _command.AddVariantsAsync(inputList);

        Assert.Equal(expected, inputList);
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }
}