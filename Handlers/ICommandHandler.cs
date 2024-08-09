using Discord.WebSocket;

namespace Kozma.net.Handlers;

public interface ICommandHandler
{
    Task HandleCommandAsync(SocketSlashCommand commandInteraction);
}
