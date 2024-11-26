using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;

namespace Kozma.net.Src.Helpers;

public class CostCalculator : ICostCalculator
{
    public double CalculateBoxCost(int amount, BoxData box)
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
