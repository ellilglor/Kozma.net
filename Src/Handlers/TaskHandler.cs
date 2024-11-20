using Discord.WebSocket;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Logging;

namespace Kozma.net.Src.Handlers;

public class TaskHandler(IBot bot, IBotLogger logger) : ITaskHandler
{
    private readonly DiscordSocketClient _client = bot.GetClient();

    public void Initialize()
    {
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
        logger.Log(LogColor.Moderation, "test");

        await Task.Delay(TimeSpan.FromMinutes(1));
        await CheckForExpiredTasksAsync();
    }
}
