using Microsoft.Extensions.DependencyInjection;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Extensions;

namespace Kozma.net.Src;

public class Program
{
    public static async Task Main()
    {
        var services = new ServiceCollection()
            .ConfigureCoreServices()
            .ConfigureHandlers()
            .ConfigureHelpers()
            .ConfigureServices()
            .BuildServiceProvider();

        await StartBotAsync(services);
    }

    private static async Task StartBotAsync(ServiceProvider services)
    {
        try
        {
            await services.GetRequiredService<IInteractionHandler>().InitializeAsync();
            AttachClientEvents(services);
            await services.GetRequiredService<IBot>().StartAsync();
            await Task.Delay(-1);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }

    private static void AttachClientEvents(ServiceProvider services)
    {
        var client = services.GetRequiredService<IBot>().GetClient();
        var interactionHandler = services.GetRequiredService<IInteractionHandler>();

        client.Log += services.GetRequiredService<IBotLogger>().HandleDiscordLog;
        client.Ready += interactionHandler.RegisterCommandsAsync;
        client.Ready += services.GetRequiredService<IRoleHandler>().CheckTradeMessagesAsync;
        client.Ready += services.GetRequiredService<ITaskHandler>().LaunchTasksAsync;
        client.InteractionCreated += interactionHandler.HandleInteractionAsync;
        client.MessageReceived += services.GetRequiredService<IMessageHandler>().HandleMessageAsync;
    }
}