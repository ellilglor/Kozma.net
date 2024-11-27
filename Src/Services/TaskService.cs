using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Src.Services;

public class TaskService(KozmaDbContext dbContext) : ITaskService
{
    public async Task<TimedTask?> GetTaskAsync(string name) =>
        await dbContext.TimedTasks.FirstOrDefaultAsync(t => t.Name == name);

    public async Task<IEnumerable<TimedTask>> GetTasksAsync(string except) =>
        await dbContext.TimedTasks.Where(t => t.Name != except).ToListAsync();

    public async Task UpdateTaskAsync(string name)
    {
        var task = await GetTaskAsync(name);
        if (task is null) return;

        task.Executed++;
        task.UpdatedAt = DateTime.Now;

        await dbContext.SaveChangesAsync();
    }
}
