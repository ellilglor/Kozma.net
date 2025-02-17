using Discord;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Models;
using Kozma.net.Src.Trackers;
using Moq;

namespace UnitTests.Helpers;

public class PunchHelperTests
{
    private readonly PunchHelper _helper;
    private readonly Mock<IPunchTracker> _punchTrackerMock;
    private readonly Mock<IFileReader> _fileReaderMock;
    private readonly Mock<IBotLogger> _loggerMock;

    public PunchHelperTests()
    {
        _punchTrackerMock = new Mock<IPunchTracker>();
        _fileReaderMock = new Mock<IFileReader>();
        _loggerMock = new Mock<IBotLogger>();

        _helper = new PunchHelper(_punchTrackerMock.Object, _fileReaderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void GetAuthor_ReturnsEmbedAuthorBuilder()
    {
        Assert.IsType<EmbedAuthorBuilder>(_helper.GetAuthor());
    }

    /*
     * Message: 
        System.NotSupportedException : Unsupported expression: x => x.ModifyOriginalResponseAsync(It.IsAny<Action<MessageProperties>>(), null)
        Non-overridable members (here: SocketInteraction.ModifyOriginalResponseAsync) may not be used in setup / verification expressions.
    [Fact]
    public async Task SendWaitingAnimationAsync_ModifiesResponse()
    {
        var embed = new EmbedBuilder().WithTitle("Temp");
        var callbackCalled = false;
        var interactionMock = new Mock<SocketInteraction>();

        interactionMock.Setup(x => x.ModifyOriginalResponseAsync(It.IsAny<Action<MessageProperties>>(), null))
        .Callback(() => callbackCalled = true)
        .Returns((Task<Discord.Rest.RestInteractionMessage>)Task.CompletedTask);

        await _helper.SendWaitingAnimationAsync(embed, interactionMock.Object, "url", delayInMs: 1);

        Assert.True(callbackCalled);
    }*/

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 3)]
    [InlineData(3, 0)]
    [InlineData(0, 3)]
    public void GetComponents_AlwaysReturnsComponents(int uvCount, int lockCount)
    {
        Assert.IsType<MessageComponent>(_helper.GetComponents(uvCount, lockCount));
    }

    [Theory]
    [InlineData(ItemType.Bomb)]
    [InlineData(ItemType.Shield)]
    [InlineData(ItemType.Armor)]
    [InlineData(ItemType.Weapon)]
    public void RollUv_ReturnsStringForEveryItemType(ItemType itemType)
    {
        var item = new PunchItem(string.Empty, itemType, string.Empty, string.Empty);

        Assert.IsType<string>(_helper.RollUv(1, item, []));
    }

    [Fact]
    public void RollUv_ReturnsExpectedFormat()
    {
        var item = new PunchItem(string.Empty, ItemType.Weapon, string.Empty, string.Empty);

        var result = _helper.RollUv(1, item, []);

        var parts = result.Split('\n');
        Assert.NotNull(parts[0]);
        Assert.NotNull(parts[1]);
        Assert.Contains(":", parts[0]);
    }

    [Fact]
    public void RollUv_ReturnsNoDuplicateUv()
    {
        var item = new PunchItem(string.Empty, ItemType.Weapon, string.Empty, string.Empty);
        var template = new List<string>() { "Charge Time Reduction" };
        var uvs = new List<string>();

        for (int i = 0; i < 1000; i++)
        {
            var uv = _helper.RollUv(1, item, template, crafting: true);

            if (!uvs.Contains(uv)) uvs.Add(uv);
        }

        Assert.DoesNotContain(uvs, x => x.Contains(template[0], StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(ItemType.Bomb, "Very High")]
    [InlineData(ItemType.Shield, "Maximum")]
    [InlineData(ItemType.Armor, "Maximum")]
    [InlineData(ItemType.Weapon, "Very High")]
    public void RollUv_ReturnsCorrectGradeForEachItemType(ItemType itemType, string expectedGrade)
    {
        var item = new PunchItem(string.Empty, itemType, string.Empty, string.Empty);
        var found = false;

        while (!found)
        {
            var uv = _helper.RollUv(1, item, []);

            if (uv.Contains(expectedGrade, StringComparison.OrdinalIgnoreCase)) found = true;
        }

        Assert.True(found);
    }

    [Fact]
    public void RollUv_GradeDistributionIsCorrect()
    {
        var item = new PunchItem(string.Empty, ItemType.Armor, string.Empty, string.Empty);
        var iterations = 100_000;
        var results = new Dictionary<string, int>
        {
            ["Maximum"] = 0,
            ["High"] = 0,
            ["Medium"] = 0,
            ["Low"] = 0
        };

        for (int i = 0; i < iterations; i++)
        {
            var uv = _helper.RollUv(1, item, []);
            var grade = results.Keys.FirstOrDefault(key => uv.Contains(key)) ?? "Unknown";

            if (results.TryGetValue(grade, out int value))
            {
                results[grade] = ++value;
            }
        }

        var veryHigh = (double)results["Maximum"] / iterations * 100;
        var high = (double)results["High"] / iterations * 100;
        var medium = (double)results["Medium"] / iterations * 100;
        var low = (double)results["Low"] / iterations * 100;

        Assert.InRange(veryHigh, 2.0, 3.0);  // 2.45%
        Assert.InRange(high, 4.5, 5.5);      // 4.87%
        Assert.InRange(medium, 19.0, 20.0);  // 19.51%
        Assert.InRange(low, 72.0, 74.0);     // 73.17%
    }

    [Theory]
    [InlineData(ItemType.Bomb, "Damage Bonus vs")]
    [InlineData(ItemType.Shield, "Increased")]
    [InlineData(ItemType.Armor, "Increased")]
    [InlineData(ItemType.Weapon, "Damage Bonus vs")]
    public void RollUv_ReturnsExpectedType(ItemType itemType, string expectedType)
    {
        var item = new PunchItem(string.Empty, itemType, string.Empty, string.Empty);
        var found = false;

        while (!found)
        {
            var uv = _helper.RollUv(1, item, []);

            if (uv.Contains(expectedType, StringComparison.OrdinalIgnoreCase)) found = true;
        }

        Assert.True(found);
    }

    [Fact]
    public void RollUv_ReturnsResistanceWhenCraftingShield()
    {
        var item = new PunchItem(string.Empty, ItemType.Shield, string.Empty, string.Empty);
        var uvs = new List<string>();

        for (int i = 0; i < 1000; i++)
        {
            var uv = _helper.RollUv(1, item, [], crafting: true);

            if (!uvs.Contains(uv)) uvs.Add(uv);
        }

        Assert.Contains(uvs, x => x.Contains("Resistance", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RollUv_ReturnsNoResistanceWhenRollingShield()
    {
        var item = new PunchItem(string.Empty, ItemType.Shield, string.Empty, string.Empty);
        var uvs = new List<string>();

        for (int i = 0; i < 1000; i++)
        {
            var uv = _helper.RollUv(1, item, [], crafting: false);

            if (!uvs.Contains(uv)) uvs.Add(uv);
        }

        Assert.DoesNotContain(uvs, x => x.Contains("Resistance", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CheckForGmAsync_ReturnsEmptyStringsIfNoGm()
    {
        var (desc, image) = await _helper.CheckForGmAsync(string.Empty, ItemType.Weapon, []);

        Assert.Empty(desc);
        Assert.Empty(image);
    }

    [Theory]
    [InlineData(ItemType.Weapon,
        new string[] { "Charge Time Reduction:\nVery High", "Attack Speed Increase:\nVery High" })]
    [InlineData(ItemType.Armor,
        new string[] { "Increased Normal Defense:\nMaximum", "Increased Shadow Defense:\nMaximum", "Increased Fire Resistance:\nMaximum" })]
    public async Task CheckForGmAsync_ReturnsStringsIfGm(ItemType itemType, string[] uvs)
    {
        _fileReaderMock.Setup(x => x.ReadAsync<IReadOnlyList<PunchReward>>(It.IsAny<string>()))
            .ReturnsAsync([new("Tony", "Stark")]);

        var (desc, image) = await _helper.CheckForGmAsync(string.Empty, itemType, uvs);

        Assert.NotEmpty(desc);
        Assert.NotEmpty(image);
    }
}
