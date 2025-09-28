using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;

namespace Kozma.net.Src.Extensions;

public static class BoxDataExtensions
{
    public static double CalculateBoxCost(this BoxData box, int amount)
    {
        switch (box.Currency)
        {
            case BoxCurrency.Energy: return amount * box.Price;
            case BoxCurrency.Dollar:
                var cost = amount / 14 * 49.95;
                amount %= 14;

                cost += amount / 5 * 19.95;
                amount %= 5;

                cost += amount * 4.95;
                return Math.Round(cost, 2);
            default: return box.Price;
        }
    }
}
