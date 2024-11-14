using Discord;
using Discord.Interactions;
using Kozma.net.Handlers;

namespace Kozma.net.Components.Buttons.FindLogsCmd;

public class Clear(IEmbedHandler embedHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("clear-messages")]
    public async Task ExecuteAsync()
    {
        var command = new Commands.Other.Clear(embedHandler);
        var embed = embedHandler.GetAndBuildEmbed("Clearing messages.");

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = new ComponentBuilder().Build();
        });

        await Commands.Other.Clear.ClearMessagesAsync(Context.User);
    }
}
