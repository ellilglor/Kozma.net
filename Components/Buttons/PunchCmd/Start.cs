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
        var components = new ComponentBuilder()
            .WithButton(emote: new Emoji("\U0001F512"), customId: "punch-info-lock", style: ButtonStyle.Primary)
            .WithButton(emote: new Emoji("1️⃣"), customId: "punch-lock-1", style: ButtonStyle.Secondary, disabled: craftedUvs.Count < 1)
            .WithButton(emote: new Emoji("2️⃣"), customId: "punch-lock-2", style: ButtonStyle.Secondary, disabled: craftedUvs.Count < 2)
            .WithButton(emote: new Emoji("3️⃣"), customId: "punch-lock-3", style: ButtonStyle.Secondary, disabled: craftedUvs.Count < 3)
            .WithButton(emote: new Emoji("\U0001F4D8"), customId: "punch-info-stats", style: ButtonStyle.Primary)
            .WithButton(emote: new Emoji("\U0001F3B2"), customId: "punch-info-gamble", style: ButtonStyle.Primary, row: 2)
            .WithButton(emote: new Emoji("1️⃣"), customId: "punch-gamble-1", style: ButtonStyle.Secondary)
            .WithButton(emote: new Emoji("2️⃣"), customId: "punch-gamble-2", style: ButtonStyle.Secondary)
            .WithButton(emote: new Emoji("3️⃣"), customId: "punch-gamble-3", style: ButtonStyle.Secondary)
            .WithButton(emote: new Emoji("❔"), customId: "punch-info-odds", style: ButtonStyle.Primary);

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed.Build();
            msg.Components = components.Build();
        });
    }
}
