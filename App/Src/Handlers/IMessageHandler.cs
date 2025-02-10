using Discord.WebSocket;

namespace Kozma.net.Src.Handlers;

public interface IMessageHandler
{
    Task HandleMessageAsync(SocketMessage rawMessage);
}
