using Discord.Interactions;
using Discord.WebSocket;

namespace Kozma.net.Components.Buttons.FindLogsCmd;

public class Clear : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("clear-messages")]
    public async Task ExecuteAsync()
    {
        // TODO: call /clear command
    }
}
