using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Interfaces.Services;

public interface ITaskService
{
    Task<TimedTask?> GetTaskAsync(string name);
    Task<IEnumerable<TimedTask>> GetTasksAsync();
    Task UpdateTaskAsync(string name);
    Task CreateTaskAsync(string name);
}
