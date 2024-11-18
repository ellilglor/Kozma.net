using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Models;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Commands.Games;

public class Punch(IEmbedHandler embedHandler, IPunchHelper punchHelper, IPunchTracker punchTracker) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Random _random = new();

    // TODO: make item enum
    [SlashCommand("punch", "Craft and roll on an item for Unique Variants.")]
    public async Task ExecuteAsync(
        [Summary(name: "item", description: "Select the item you want to craft."),
            Choice("Brandish", "Brandish"),
            Choice("Overcharged Mixmaster", "Overcharged Mixmaster"),
            Choice("Blast Bomb", "Blast Bomb"),
            Choice("Swiftstrike Buckler", "Swiftstrike Buckler"),
            Choice("Black Kat Cowl", "Black Kat Cowl")] string item)
    {
        punchTracker.SetPlayer(Context.User.Id, item);
        await CraftItemAsync(Context.Interaction, Context.User.Id, punchHelper.ConvertToPunchOption(item));
    }

    public async Task CraftItemAsync(SocketInteraction interaction, ulong userId, PunchOption? item, int counter = 1)
    {
        if (item is null)
        {
            await interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embedHandler.GetAndBuildEmbed("Something went wrong while crafting"));
            return;
        }

        var itemData = punchHelper.GetItem((PunchOption)item)!;
        var craftUvs = CraftItem(userId, itemData);
        var fields = craftUvs.Select((uv, index) => embedHandler.CreateField($"UV #{index + 1}", uv)).ToList();
        fields.Add(embedHandler.CreateField("Crafted", counter.ToString(), inline: false));

        var (desc, image) = await punchHelper.CheckForGmAsync(interaction.User.Username, itemData.Type, craftUvs);
        var embed = embedHandler.GetEmbed($"You crafted: {itemData.Name}")
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(itemData.Image)
            .WithDescription(desc)
            .WithImageUrl(image)
            .WithFields(fields);
        var components = new ComponentBuilder()
            .WithButton(label: "Recraft", customId: "recraft", style: ButtonStyle.Primary)
            .WithButton(label: "Start Rolling Uvs", customId: "start-punching", style: ButtonStyle.Primary);

        await punchHelper.SendWaitingAnimationAsync(embedHandler.GetEmbed(string.Empty), interaction, "https://cdn.discordapp.com/attachments/1069643121622777876/1069643186978430996/crafting.gif", 2500);

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
