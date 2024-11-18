using Kozma.net.Src.Enums;
using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Services;

public interface IUnboxService
{
    public Task UpdateOrSaveBoxAsync(Box box);
    public Task<int> GetBoxOpenedCountAsync();
    public Task<IEnumerable<UnboxStat>> GetBoxesAsync(int total);
}
