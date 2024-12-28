using Discord.Interactions;
using DotNetEnv;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Services;
using Kozma.net.Src.Trackers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kozma.net.Src.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureCoreServices(this IServiceCollection services)
    {
        Env.TraversePath().Load();

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        return services
            .AddMemoryCache()
            .AddSingleton(config)
            .AddSingleton<IBot, Bot>()
            .AddSingleton<IBotLogger, Logger>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<IBot>().GetClient()))
            .AddDbContext<KozmaDbContext>(options => options.UseMongoDB(Env.GetString("dbToken"), Env.GetString("database")), contextLifetime: ServiceLifetime.Transient);
    }

    public static IServiceCollection ConfigureHandlers(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEmbedHandler, EmbedHandler>()
            .AddSingleton<IInteractionHandler, InteractionHandler>()
            .AddSingleton<IMessageHandler, MessageHandler>()
            .AddSingleton<IRoleHandler, RoleHandler>()
            .AddSingleton<ITaskHandler, TaskHandler>()
            .AddSingleton<IRateLimitHandler, RateLimitHandler>();
    }

    public static IServiceCollection ConfigureHelpers(this IServiceCollection services)
    {
        return services
            .AddSingleton<ICostCalculator, CostCalculator>()
            .AddSingleton<IPunchHelper, PunchHelper>()
            .AddSingleton<IUpdateHelper, UpdateHelper>()
            .AddSingleton<IFileReader, JsonFileReader>()
            .AddSingleton<IUnboxTracker, UnboxTracker>()
            .AddSingleton<IPunchTracker, PunchTracker>()
            .AddSingleton<IStatPageTracker, StatPageTracker>();
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<ITradeLogService, TradeLogService>()
            .AddSingleton<IExchangeService, ExchangeService>()
            .AddSingleton<ICommandService, CommandService>()
            .AddSingleton<IUserService, UserService>()
            .AddSingleton<IUnboxService, UnboxService>()
            .AddSingleton<IPunchService, PunchService>()
            .AddSingleton<ITaskService, TaskService>();
    }
}
