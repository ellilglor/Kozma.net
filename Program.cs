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
                .BuildServiceProvider();

        await StartBot(serviceProvider);
    }

    private static async Task StartBot(ServiceProvider serviceProvider)
    {
        try
        {
            var bot = serviceProvider.GetRequiredService<IBot>();
            await bot.StartAsync();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            Environment.Exit(-1);
        }

        await Task.Delay(-1);
    }
}