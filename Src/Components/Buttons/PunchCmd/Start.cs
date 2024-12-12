using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;

namespace Kozma.net.Src.Components.Buttons.PunchCmd;

public class Start(IEmbedHandler embedHandler, IPunchHelper punchHelper) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("start-punching")]
    public async Task ExecuteAsync()
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var itemData = context.Message.Embeds.First().Title.Replace("You crafted: ", string.Empty, StringComparison.OrdinalIgnoreCase).ConvertToPunchOption().ToPunchItem();
        var craftedUvs = context.Message.Embeds.First().Fields.Where(f => f.Name.Contains("UV", StringComparison.OrdinalIgnoreCase)).ToList();

        var fields = craftedUvs.Select(field => embedHandler.CreateField($"{Emotes.Unlocked} {field.Name}", field.Value)).ToList();
        fields.Add(embedHandler.CreateField("Crowns Spent", "0", isInline: false));

        var embed = embedHandler.GetEmbed(itemData.Name)
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(itemData.Image)
            .WithFields(fields);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed.Build();
            msg.Components = punchHelper.GetComponents(craftedUvs.Count);
        });
    }
}
