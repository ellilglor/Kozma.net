using Discord.WebSocket;

namespace Kozma.net.Src.Interfaces.Handlers;

public interface IInteractionHandler
{
    Task InitializeAsync();
    Task RegisterCommandsAsync();
    Task HandleInteractionAsync(SocketInteraction interaction);
}
