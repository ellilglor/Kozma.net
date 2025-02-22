using Kozma.net.Src.Enums;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Models;

namespace UnitTests.Helpers;

public class CostCalculatorTests
{
    private readonly CostCalculator _costCalculator;

    public CostCalculatorTests()
    {
        _costCalculator = new CostCalculator();
    }

    [Fact]
    public void CalculateBoxCost_Energy_ReturnsAmountTimesPrice()
    {
        var box = new BoxData(1, BoxCurrency.Energy, string.Empty, string.Empty, string.Empty);
        var amount = 5;
        var expectedCost = amount * box.Price;

        var result = _costCalculator.CalculateBoxCost(amount, box);

        Assert.Equal(expectedCost, result);
    }

    [Fact]
    public void CalculateBoxCost_Dollar_Single()
    {
        var box = new BoxData(0, BoxCurrency.Dollar, string.Empty, string.Empty, string.Empty);

        var result = _costCalculator.CalculateBoxCost(1, box);

        Assert.Equal(4.95, result);
    }

    [Fact]
    public void CalculateBoxCost_Dollar_FirstBulk()
    {
        var box = new BoxData(0, BoxCurrency.Dollar, string.Empty, string.Empty, string.Empty);
        var amountForFirstBulkDiscount = 5;

        var result = _costCalculator.CalculateBoxCost(amountForFirstBulkDiscount, box);

        Assert.Equal(19.95, result);
    }


    [Fact]
    public void CalculateBoxCost_DollarCurrency_BulkBox()
    {
        var box = new BoxData(0, BoxCurrency.Dollar, string.Empty, string.Empty, string.Empty);
        var amountForSecondBulkDiscount = 14;

        var result = _costCalculator.CalculateBoxCost(amountForSecondBulkDiscount, box);

        Assert.Equal(49.95, result);
    }

    [Fact]
    public void CalculateBoxCost_Dollar_CalculatesBulkDiscounts()
    {
        // 14 boxes at bulk rate (49.95)
        // 5 boxes at medium rate (19.95)
        // 1 box at single rate (4.95)
        var expected = 49.95 + 19.95 + 4.95;
        var box = new BoxData(0, BoxCurrency.Dollar, string.Empty, string.Empty, string.Empty);
        var MinimumAmountForCalculation = 20;

        var result = _costCalculator.CalculateBoxCost(MinimumAmountForCalculation, box);

        Assert.Equal(Math.Round(expected, 2), result);
    }


    [Fact]
    public void CalculateBoxCost_UnknownCurrency_ReturnsBoxPrice()
    {
        var box = new BoxData(int.MaxValue, (BoxCurrency)999, string.Empty, string.Empty, string.Empty);

        var result = _costCalculator.CalculateBoxCost(0, box);

        Assert.Equal(box.Price, result);
    }


    [Theory]
    [InlineData(BoxCurrency.Energy)]
    [InlineData(BoxCurrency.Dollar)]
    public void CalculateBoxCost_ZeroAmount_ReturnsZero(BoxCurrency currency)
    {
        var box = new BoxData(0, currency, string.Empty, string.Empty, string.Empty);

        var result = _costCalculator.CalculateBoxCost(0, box);

        Assert.Equal(0, result);
    }
}
