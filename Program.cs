using Kozma.net.Handlers;
using Kozma.net.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kozma.net;

public class Program
{
    public static async Task Main()
    {
        var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfigFactory, ConfigFactory>()
                .AddSingleton<IBot, Bot>()
                .AddSingleton<ICommandHandler, CommandHandler>()
                .BuildServiceProvider();

        await StartBotAsync(serviceProvider);
    }

    private static async Task StartBotAsync(ServiceProvider serviceProvider)
    {
        try
        {
            var bot = serviceProvider.GetRequiredService<IBot>();

            await bot.StartAsync(serviceProvider);
            await Task.Delay(-1);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            Environment.Exit(-1);
        }
    }
}