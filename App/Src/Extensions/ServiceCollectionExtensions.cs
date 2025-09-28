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
            .AddScoped<IBotLogger, Logger>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<IBot>().Client))
            .AddDbContext<KozmaDbContext>(options => options.UseMongoDB(Env.GetString("dbToken"), Env.GetString("database")), contextLifetime: ServiceLifetime.Transient);
    }

    public static IServiceCollection ConfigureHandlers(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEmbedHandler, EmbedHandler>()
            .AddScoped<IInteractionHandler, InteractionHandler>()
            .AddScoped<IMessageHandler, MessageHandler>()
            .AddScoped<IRoleHandler, RoleHandler>()
            .AddScoped<ITaskHandler, TaskHandler>()
            .AddSingleton<IRateLimitHandler, RateLimitHandler>();
    }

    public static IServiceCollection ConfigureHelpers(this IServiceCollection services)
    {
        return services
            .AddScoped<IPunchHelper, PunchHelper>()
            .AddScoped<IUpdateHelper, UpdateHelper>()
            .AddSingleton<IFileReader, JsonFileReader>()
            .AddSingleton<IDiscordPaginator, DiscordPaginator>()
            .AddSingleton<IApiFetcher, ApiFetcher>()
            .AddSingleton<IUnboxTracker, UnboxTracker>()
            .AddSingleton<IPunchTracker, PunchTracker>();
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        return services
            .AddScoped<ITradeLogService, TradeLogService>()
            .AddScoped<IExchangeService, ExchangeService>()
            .AddScoped<ICommandService, CommandService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<IUnboxService, UnboxService>()
            .AddScoped<IPunchService, PunchService>()
            .AddScoped<ITaskService, TaskService>();
    }
}
