using Discord.WebSocket;

namespace Kozma.net.Commands;

public interface ICommand
{
    Task ExecuteAsync(SocketSlashCommand command);
}
