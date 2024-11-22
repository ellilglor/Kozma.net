using Discord.WebSocket;

namespace Kozma.net.Src.Handlers;

public interface IInteractionHandler
{
    Task InitializeAsync();
    Task RegisterCommandsAsync();
    Task HandleInteractionAsync(SocketInteraction interaction);
}
