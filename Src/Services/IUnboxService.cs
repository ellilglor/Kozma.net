using Kozma.net.Src.Enums;
using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Services;

public interface IUnboxService
{
    Task UpdateOrSaveBoxAsync(Box box);
    Task<int> GetBoxOpenedCountAsync();
    Task<IEnumerable<UnboxStat>> GetBoxesAsync(int total);
}
