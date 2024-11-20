using Discord.WebSocket;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Services;

namespace Kozma.net.Src.Handlers;

public class TaskHandler(IBot bot, IBotLogger logger, ITaskService taskService) : ITaskHandler
{
    private record TaskConfig(double Interval, Func<Task> ExecuteAsync);

    private readonly DiscordSocketClient _client = bot.GetClient();

    private readonly Dictionary<string, TaskConfig> _tasks = new();

    public void Initialize()
    {
        _tasks.Add("energyMarket", new TaskConfig(12, async () => await PostEnergyMarketAsync()));
        _tasks.Add("slowmodeReminder", new TaskConfig(36, async () => await PostSlowModeReminderAsync()));
        _tasks.Add("scamPrevention", new TaskConfig(72, async () => await PostScamPreventionAsync()));
        _tasks.Add("newLogs", new TaskConfig(6, async () => await CheckForNewLogsAsync()));

        _client.Ready += OnReadyAsync;
    }

    private async Task OnReadyAsync()
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () => await CheckForExpiredTasksAsync());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        await Task.CompletedTask;
    }

    private async Task CheckForExpiredTasksAsync()
    {
        //logger.Log(LogColor.Moderation, "test");
        // TODO check expired mutes
        var tasks = await taskService.GetTasksAsync(except: "offlineMutes");
        var currentTime = DateTime.Now;

        foreach (var task in tasks)
        {
            var config = _tasks[task.Name];
            if (task.UpdatedAt.AddHours(config.Interval) > currentTime) continue;

            await config.ExecuteAsync();
            await taskService.UpdateTaskAsync(task.Name);
        }

        await Task.Delay(TimeSpan.FromMinutes(1));
        await CheckForExpiredTasksAsync();
    }

    private async Task PostEnergyMarketAsync()
    {
        logger.Log(Enums.LogColor.Moderation, "energy");
        await Task.CompletedTask;
    }

    private async Task PostSlowModeReminderAsync()
    {
        logger.Log(Enums.LogColor.Moderation, "slow");
        await Task.CompletedTask;
    }

    private async Task PostScamPreventionAsync()
    {
        logger.Log(Enums.LogColor.Moderation, "scam");
        await Task.CompletedTask;
    }

    private async Task CheckForNewLogsAsync()
    {
        logger.Log(Enums.LogColor.Moderation, "logs");
        await Task.CompletedTask;
    }
}
