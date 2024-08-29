using Kozma.net.Handlers;
using Kozma.net.Factories;
using Microsoft.Extensions.DependencyInjection;
using Discord.Interactions;
using Kozma.net.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Kozma.net.Helpers;

namespace Kozma.net;

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
                .AddSingleton(x => new InteractionService(x.GetRequiredService<IBot>().GetClient()))
                .AddSingleton<IEmbedFactory, EmbedFactory>()
                .AddSingleton<IInteractionHandler, InteractionHandler>()
                .AddSingleton<IboxHelper, BoxHelper>()
                .AddSingleton<IContentHelper, ContentHelper>()
                .AddDbContext<KozmaDbContext>(options => options.UseMongoDB(config.GetValue<string>("dbToken") ?? string.Empty, config.GetValue<string>("database") ?? string.Empty))
                .AddScoped<ITradeLogService, TradeLogService>()
                .AddScoped<IExchangeService, ExchangeService>()
                .BuildServiceProvider();

        await services.GetRequiredService<IInteractionHandler>().InitializeAsync();
        await StartBotAsync(services);
    }

    private static async Task StartBotAsync(ServiceProvider services)
    {
        try
        {
            var bot = services.GetRequiredService<IBot>();

            await bot.StartAsync();
            await Task.Delay(-1);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
}