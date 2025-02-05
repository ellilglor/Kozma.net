﻿using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Kozma.net.Src;

internal sealed class Program
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
        var bot = services.GetRequiredService<IBot>();
        using (services)
        {
            await services.GetRequiredService<IInteractionHandler>().InitializeAsync();
            AttachClientEvents(services);
            await bot.StartAsync();
            await Task.Delay(-1);
        }

        bot.Dispose();
    }

    private static void AttachClientEvents(ServiceProvider services)
    {
        var client = services.GetRequiredService<IBot>().Client;
        var interactionHandler = services.GetRequiredService<IInteractionHandler>();

        client.Log += services.GetRequiredService<IBotLogger>().HandleDiscordLog;
        //client.Ready += interactionHandler.RegisterCommandsAsync;
        client.Ready += services.GetRequiredService<IRoleHandler>().CheckTradeMessagesAsync;
        client.Ready += services.GetRequiredService<ITaskHandler>().LaunchTasksAsync;
        client.InteractionCreated += interactionHandler.HandleInteractionAsync;
        client.MessageReceived += services.GetRequiredService<IMessageHandler>().HandleMessageAsync;
    }
}