using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Enums;
using Kozma.net.Handlers;
using Kozma.net.Helpers;
using Kozma.net.Trackers;
namespace Kozma.net.Components.Buttons.UnboxCmd;

public class Unbox(IEmbedHandler embedHandler, IBoxHelper boxHelper, IUnboxTracker unboxTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("unbox-*")]
    public async Task ExecuteAsync(string action)
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var embed = context.Message.Embeds.First();

        if (Enum.TryParse(embed.Author!.Value.Name, out Box box))
        {
            if (string.Equals(action, "again"))
            {
                var command = new Commands.Games.Unbox(embedHandler, boxHelper, unboxTracker);
                await command.UnboxAsync(Context, box, int.Parse(embed.Fields[0].Value) + 1);
            }
            else
            {
                var boxData = boxHelper.GetBox(box)!;
                var fields = new List<EmbedFieldBuilder>
                {
                    embedHandler.CreateField(embed.Fields[0].Name, embed.Fields[0].Value),
                    embedHandler.CreateField(embed.Fields[1].Name, embed.Fields[1].Value),
                    embedHandler.CreateField("\u200b", "\u200b"),
                    embedHandler.CreateField("Unique", $"{unboxTracker.GetItemCount(Context.User.Id, box)}"),
                    embedHandler.CreateField("Info", $"[Link]({boxData.Page} 'page with distribution of probabilities')"),
                    embedHandler.CreateField("\u200b", "\u200b")
                };

                var statEmbed = embedHandler.GetEmbed("In this session you opened:")
                    .WithAuthor(new EmbedAuthorBuilder().WithName(box.ToString()).WithIconUrl(boxData.Url))
                    .WithDescription(unboxTracker.GetData(Context.User.Id, box))
                    .WithFields(fields)
                    .Build();

                await ModifyOriginalResponseAsync(msg => msg.Embed = statEmbed);
            }
        } 
        else
        {
            await ModifyOriginalResponseAsync(msg => {
                msg.Embed = embedHandler.GetAndBuildEmbed($"Something went wrong while trying to get the box from {box}"); ;
                msg.Components = new ComponentBuilder().Build();
            });
        }
    }
}
