using Kozma.net.Src.Models;

namespace Kozma.net.Src.Helpers;

public interface ICostCalculator
{
    double CalculateBoxCost(int amount, BoxData box);
}
