using Microsoft.Extensions.DependencyInjection;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Kozma.net.Helpers;
using Kozma.net.Logging;
using Kozma.net.Src.Trackers;
using Kozma.net.Src.Services;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Handlers;

namespace Kozma.net.Src;

public class Program
{
    public static async Task Main()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

        var services = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<IBot, Bot>()
            .AddSingleton<IBotLogger, Logger>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<IBot>().GetClient()))
            .AddSingleton<IEmbedHandler, EmbedHandler>()
            .AddSingleton<IInteractionHandler, InteractionHandler>()
            .AddSingleton<IMessageHandler, MessageHandler>()
            .AddSingleton<IRoleHandler, RoleHandler>()
            .AddSingleton<ITaskHandler, TaskHandler>()
            .AddSingleton<IBoxHelper, BoxHelper>()
            .AddSingleton<IPunchHelper, PunchHelper>()
            .AddSingleton<IUpdateHelper, UpdateHelper>()
            .AddSingleton<IContentHelper, ContentHelper>()
            .AddSingleton<IFileReader, JsonFileReader>()
            .AddSingleton<IUnboxTracker, UnboxTracker>()
            .AddSingleton<IPunchTracker, PunchTracker>()
            .AddSingleton<IStatPageTracker, StatPageTracker>()
            .AddDbContext<KozmaDbContext>(options => options.UseMongoDB(config.GetValue<string>("dbToken") ?? string.Empty, config.GetValue<string>("database") ?? string.Empty))
            .AddScoped<ITradeLogService, TradeLogService>()
            .AddScoped<IExchangeService, ExchangeService>()
            .AddScoped<ICommandService, CommandService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<IUnboxService, UnboxService>()
            .AddScoped<IPunchService, PunchService>()
            .AddScoped<ITaskService, TaskService>()
            .BuildServiceProvider();

        await services.GetRequiredService<IInteractionHandler>().InitializeAsync();
        AttachClientEvents(services);
        await StartBotAsync(services);
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

    private static async Task StartBotAsync(ServiceProvider services)
    {
        try
        {
            await services.GetRequiredService<IBot>().StartAsync();
            await Task.Delay(-1);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
}