using Discord.WebSocket;
using Kozma.net.Commands;
using Kozma.net.Commands.Information;
using Kozma.net.Services;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Handlers;

public class CommandHandler : ICommandHandler
{
    private readonly IConfiguration _config;

    public CommandHandler(IConfigFactory configFactory)
    {
        _config = configFactory.GetConfig();
    }

    public async Task HandleCommandAsync(SocketSlashCommand commandInteraction)
    {
        if (commandInteraction.User.Id != _config.GetValue<ulong>("ids:ownerId"))
        {
            await commandInteraction.RespondAsync("The bot is currently being worked on.\nPlease try again later.", ephemeral: true);
            return;
        }

        await commandInteraction.DeferAsync(ephemeral: true);

        // TODO: check if banned from server

        var command = GetCommand(commandInteraction.Data.Name);

        if (command == null)
        {
            await commandInteraction.ModifyOriginalResponseAsync(msg => msg.Content = "It looks like this command is missing!");
            return;
        }

        await command.ExecuteAsync(commandInteraction);
    }

    private static ICommand? GetCommand(string commandName)
    {
        return commandName switch
        {
            "help" => new Help(),
            _ => null
        };
    }
}
