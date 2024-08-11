using Kozma.net.Handlers;
using Kozma.net.Factories;
using Microsoft.Extensions.DependencyInjection;
using Discord.Interactions;
using Discord.WebSocket;
using Discord;

namespace Kozma.net;

public class Program
{
    public static async Task Main()
    {
        DiscordSocketConfig config = new()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
        };

        var services = new ServiceCollection()
                .AddSingleton<IConfigFactory, ConfigFactory>()
                .AddSingleton<IBot, Bot>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<IBot>().GetClient()))
                .AddSingleton<IEmbedFactory, EmbedFactory>()
                .AddSingleton<IInteractionHandler, InteractionHandler>()
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