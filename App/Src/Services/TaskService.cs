using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

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

    public async Task CreateTaskAsync(string name)
    {
        var task = new TimedTask()
        {
            Id = ObjectId.GenerateNewId(),
            Name = name,
            Executed = 0,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await dbContext.TimedTasks.AddAsync(task);
        await dbContext.SaveChangesAsync();
    }
}
