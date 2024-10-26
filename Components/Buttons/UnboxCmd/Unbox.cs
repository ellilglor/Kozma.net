using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Enums;
using Kozma.net.Factories;
using Kozma.net.Helpers;

namespace Kozma.net.Components.Buttons.UnboxCmd;

public class Unbox(IEmbedFactory embedFactory, IboxHelper boxHelper) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("unbox-*")]
    public async Task ExecuteAsync(string action)
    {
        if (string.Equals(action, "again"))
        {
            var context = (SocketMessageComponent)Context.Interaction;
            var command = new Commands.Games.Unbox(embedFactory, boxHelper);
            var embed = context.Message.Embeds.First();
            var amount = int.Parse(embed.Fields[0].Value) + 1;

            if (Enum.TryParse(embed.Author!.Value.Name, out Box box))
            {
                await command.UnboxAsync(Context, box, amount);
            }
            else
            {
                await ModifyOriginalResponseAsync(msg => {
                    msg.Embed = embedFactory.GetAndBuildEmbed($"Something went wrong while trying to get the box from {box}"); ;
                    msg.Components = new ComponentBuilder().Build();
                });
            }
        }
    }
}
