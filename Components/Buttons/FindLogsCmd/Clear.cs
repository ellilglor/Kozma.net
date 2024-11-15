using Discord.Interactions;
using Kozma.net.Handlers;

namespace Kozma.net.Components.Buttons.FindLogsCmd;

public class Clear(IEmbedHandler embedHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("clear-messages")]
    public async Task ExecuteAsync()
    {
        var command = new Commands.Other.Clear(embedHandler);

        await command.RespondAsync(Context.Interaction);
        await command.ClearMessagesAsync(Context.User);
    }
}
