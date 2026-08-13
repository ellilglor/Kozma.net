using Discord.WebSocket;

namespace Kozma.net.Src.Interfaces.Handlers;

public interface IMessageHandler
{
    Task HandleMessageAsync(SocketMessage rawMessage);
}
