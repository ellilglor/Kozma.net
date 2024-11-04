using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Enums;
using Kozma.net.Factories;
using Kozma.net.Helpers;

namespace Kozma.net.Components.Buttons.PunchCmd;

public class Start(IEmbedFactory embedFactory, IPunchHelper punchHelper) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("start-punching")]
    public async Task ExecuteAsync()
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var item = punchHelper.ConvertToPunchOption(context.Message.Embeds.First().Title.Replace("You crafted: ", string.Empty))!;
        var itemData = punchHelper.GetItem((PunchOption)item)!;
        var craftedUvs = context.Message.Embeds.First().Fields.Where(f => f.Name.Contains("UV")).ToList();
        var fields = new List<EmbedFieldBuilder>();

        foreach (var field in craftedUvs)
        {
            fields.Add(embedFactory.CreateField($"\U0001f513 {field.Name}", field.Value));
        }
        fields.Add(embedFactory.CreateField("Crowns Spent", "0", inline: false));

        var embed = embedFactory.GetEmbed(itemData.Name)
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(itemData.Image)
            .WithFields(fields);

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed.Build();
            msg.Components = punchHelper.GetComponents(craftedUvs.Count < 1, craftedUvs.Count < 2, craftedUvs.Count < 3);
        });
    }
}
