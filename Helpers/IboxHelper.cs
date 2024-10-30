using Kozma.net.Enums;
using Kozma.net.Models;

namespace Kozma.net.Helpers;

public interface IboxHelper
{
    BoxData? GetBox(Box box);
    string GetBoxImage(Box box);
    Box? ConvertLockboxOption(LockboxOption box);
    Task<List<ItemData>> GetItemDataAsync(Box box);
}
