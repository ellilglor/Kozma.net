using Kozma.net.Src.Models;
using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Services;

public interface IUserService
{
    public Task UpdateOrSaveUserAsync(ulong id, string name, bool isCommand, bool isUnbox);
    public Task<bool> SaveMuteAsync<T>(ulong id, DateTime createdAt, Func<T> factory) where T : Mute;
    public Task<IEnumerable<T>> GetAndDeleteExpiredMutesAsync<T>() where T : Mute;
    public Task<int> GetTotalUsersCountAsync();
    public Task<IEnumerable<DbStat>> GetUsersAsync(int limit, int total, bool forUnboxed);
}
