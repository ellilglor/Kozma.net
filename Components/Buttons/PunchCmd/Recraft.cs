using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Commands.Games;
using Kozma.net.Factories;
using Kozma.net.Helpers;
using Kozma.net.Trackers;

namespace Kozma.net.Components.Buttons.PunchCmd;

public class Recraft(IEmbedFactory embedFactory, IPunchHelper punchHelper, IPunchTracker punchTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("recraft")]
    public async Task ExecuteAsync()
    {
        var command = new Punch(embedFactory, punchHelper, punchTracker);
        var context = (SocketMessageComponent)Context.Interaction;
        var item = punchHelper.ConvertToPunchOption(context.Message.Embeds.First().Title.Replace("You crafted: ", string.Empty));
        var amount = int.Parse(context.Message.Embeds.First().Fields.First(f => f.Name == "Crafted").Value) + 1;
        
        await command.CraftItemAsync(Context, item, amount);
    }
}
