using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Models;

namespace UnitTests.Extensions;

public class EnumExtensionsTests
{
    [Fact]
    public void ToBoxData_ReturnsDataIfValidBox()
    {
        var result = Box.Confection.ToBoxData();

        Assert.IsType<BoxData>(result);
    }

    [Fact]
    public void ToBoxData_ThrowsExceptionWhenInvalidBox()
    {
        var box = (Box)int.MaxValue;

        Assert.Throws<InvalidCastException>(() => box.ToBoxData());
    }

    [Fact]
    public void ConvertToBox_ReturnsBoxIfValidOption()
    {
        var result = LockboxOption.Iron.ConvertToBox();

        Assert.IsType<Box>(result);
    }

    [Fact]
    public void ConvertToBox_ThrowsExceptionWhenInvalidOption()
    {
        var option = (LockboxOption)int.MaxValue;

        Assert.Throws<InvalidCastException>(() => option.ConvertToBox());
    }

    [Fact]
    public void ToPunchItem_ReturnsItemIfValidOption()
    {
        var result = PunchOption.Mixmaster.ToPunchItem();

        Assert.IsType<PunchItem>(result);
    }

    [Fact]
    public void ToPunchItem_ThrowsExceptionWhenInvalidOption()
    {
        var item = (PunchOption)int.MaxValue;

        Assert.Throws<InvalidCastException>(() => item.ToPunchItem());
    }

    [Fact]
    public void Color_Valid_ReturnsString()
    {
        var result = LogLevel.Special.Color();

        Assert.IsType<string>(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Color_Invalid_ReturnsString()
    {
        var level = (LogLevel)int.MaxValue;

        var result = level.Color();

        Assert.IsType<string>(result);
        Assert.NotEmpty(result);
    }
}
