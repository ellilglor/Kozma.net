using Kozma.net.Src.Commands.Information;
using Kozma.net.Src.Handlers;
using Moq;

namespace UnitTests.Commands;

public class LockboxTests
{
    private readonly Lockbox _command;
    private readonly Mock<IEmbedHandler> _embedHandlerMock;
    // https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/edit?gid=887295083#gid=887295083

    public LockboxTests()
    {
        _embedHandlerMock = new Mock<IEmbedHandler>();

        _command = new Lockbox(_embedHandlerMock.Object);
    }

    [Fact]
    public void FindItem_CanFindMatch()
    {
        var item = "Vitakit";
        var expected = "Silver";

        var result = _command.FindItem(item);

        Assert.Contains(expected, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FindItem_Iron_MentionsPool()
    {
        var item = "Vitakit";
        var expected = "Inside 20% pool";

        var result = _command.FindItem(item);

        Assert.Contains(expected, result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FindItem_Iron_MentionsBothPool()
    {
        var item = "Wings";
        var expected1 = "Inside 20% pool";
        var expected2 = "Inside 80% pool";

        var result = _command.FindItem(item);

        Assert.Contains(expected1, result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(expected2, result, StringComparison.OrdinalIgnoreCase);
    }
}
