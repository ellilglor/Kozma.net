using Discord.WebSocket;

namespace Kozma.net.Commands.Information;

public class Help : ICommand
{

    public Help()
    {

    }

    public async Task ExecuteAsync(SocketSlashCommand command)
    {
        await command.ModifyOriginalResponseAsync(msg => msg.Content = $"You executed {command.Data.Name}");
    }
}
