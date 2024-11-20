using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Services;

public interface ITaskService
{
    Task<TimedTask?> GetTaskAsync(string name);
    Task UpdateTaskAsync(string name);
}
