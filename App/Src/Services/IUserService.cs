using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Services;

public interface IUserService
{
    Task UpdateOrSaveUserAsync(ulong id, string name, bool isCommand, string command);
    Task<bool> SaveMuteAsync(ulong id, string name, bool isWtb, DateTime msgCreatedAt);
    Task<IEnumerable<Mute>> GetAndDeleteExpiredMutesAsync();
    Task<IEnumerable<Mute>> GetMutesAsync();
    Task<int> GetTotalUsersCountAsync();
    Task<IEnumerable<DbStat>> GetUsersAsync(int limit, int total, bool forUnboxed);
}
