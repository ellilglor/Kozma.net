using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Commands.Games;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Components.Buttons.PunchCmd;

public class Recraft(IEmbedHandler embedHandler, IPunchHelper punchHelper, IPunchTracker punchTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("recraft")]
    public async Task ExecuteAsync()
    {
        var command = new Punch(embedHandler, punchHelper, punchTracker);
        var context = (SocketMessageComponent)Context.Interaction;
        var item = context.Message.Embeds.First().Title.Replace("You crafted: ", string.Empty, StringComparison.OrdinalIgnoreCase).ConvertToPunchOption();
        var amount = int.Parse(context.Message.Embeds.First().Fields.First(f => f.Name == "Crafted").Value) + 1;

        await command.CraftItemAsync(Context.Interaction, Context.User.Id, item, amount);
    }
}
