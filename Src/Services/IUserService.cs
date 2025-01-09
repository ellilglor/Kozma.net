using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Services;

public interface IUserService
{
    Task UpdateOrSaveUserAsync(ulong id, string name, bool isCommand, string command);
    Task<bool> SaveMuteAsync<T>(ulong id, DateTime createdAt, Func<T> factory) where T : Mute;
    Task<IEnumerable<T>> GetAndDeleteExpiredMutesAsync<T>() where T : Mute;
    Task<int> GetTotalUsersCountAsync();
    Task<IEnumerable<DbStat>> GetUsersAsync(int limit, int total, bool forUnboxed);
}
