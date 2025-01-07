using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Models;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Commands.Games;

public class Punch(IEmbedHandler embedHandler, IPunchHelper punchHelper, IPunchTracker punchTracker) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Random _random = new();

    [SlashCommand("punch", "Craft and roll on an item for Unique Variants.")]
    public async Task ExecuteAsync(
        [Summary(name: "item", description: "Select the item you want to craft.")] PunchOption choice)
    {
        var item = choice.ToPunchItem();
        punchTracker.SetPlayer(Context.User.Id, item.Name);
        await CraftItemAsync(Context.Interaction, Context.User.Id, item);
    }

    public async Task CraftItemAsync(SocketInteraction interaction, ulong userId, PunchItem item, int counter = 1)
    {
        var craftUvs = CraftItem(userId, item);
        var fields = craftUvs.Select((uv, index) => embedHandler.CreateField($"UV #{index + 1}", uv)).ToList();
        fields.Add(embedHandler.CreateField("Crafted", counter.ToString(), isInline: false));

        var (desc, image) = await punchHelper.CheckForGmAsync(interaction.User.Username, item.Type, craftUvs);
        var embed = embedHandler.GetEmbed($"You crafted: {item.Name}")
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(item.Image)
            .WithDescription(desc)
            .WithImageUrl(image)
            .WithFields(fields);
        var components = new ComponentBuilder()
            .WithButton(label: "Recraft", customId: "recraft", style: ButtonStyle.Primary)
            .WithButton(label: "Start Rolling Uvs", customId: "start-punching", style: ButtonStyle.Primary);

        await punchHelper.SendWaitingAnimationAsync(embedHandler.GetEmbed(string.Empty), interaction, "https://cdn.discordapp.com/attachments/1069643121622777876/1069643186978430996/crafting.gif", delayInMs: 2500);

        await interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed.Build();
            msg.Components = components.Build();
        });
    }

    /*
     * Chances:
     * 1/1000 for 3 UVs
     * 1/100 for 2 Uvs
     * 1/10 for 1 UV
     * */
    private List<string> CraftItem(ulong id, PunchItem item)
    {
        int craftRoll = _random.Next(1, 1001);
        var limit = craftRoll == 1 ? 3 : craftRoll <= 11 ? 2 : craftRoll <= 111 ? 1 : 0;
        var uvs = new List<string>();

        for (int i = 0; i < limit; i++)
        {
            uvs.Add(punchHelper.RollUv(id, item, uvs, crafting: true));
        }

        return uvs;
    }
}
