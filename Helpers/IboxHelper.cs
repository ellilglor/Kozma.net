using Kozma.net.Enums;
using Kozma.net.Models;

namespace Kozma.net.Helpers;

public interface IBoxHelper
{
    BoxData? GetBox(Box box);
    string GetBoxImage(Box box);
    Box? ConvertLockboxOption(LockboxOption box);
    double CalculateCost(int amount, BoxData box);
    Task<List<ItemData>> GetItemDataAsync(Box box);
}
