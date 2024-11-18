using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;

namespace Kozma.net.Src.Helpers;

public interface IBoxHelper
{
    BoxData? GetBox(Box box);
    Box? ConvertLockboxOption(LockboxOption box);
    double CalculateCost(int amount, BoxData box);
    Task<List<ItemData>> GetItemDataAsync(Box box);
}
