using Discord;
using Discord.Interactions;
using Kozma.net.Factories;

namespace Kozma.net.Components.Buttons.FindLogsCmd;

public class Clear(IEmbedFactory embedFactory) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("clear-messages")]
    public async Task ExecuteAsync()
    {
        var command = new Commands.Other.Clear(embedFactory);
        var embed = embedFactory.GetAndBuildEmbed("Clearing messages.");

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = new ComponentBuilder().Build();
        });

        await Commands.Other.Clear.ClearMessagesAsync(Context.User);
    }
}
