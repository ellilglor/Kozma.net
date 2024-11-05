using Discord;
using Discord.Interactions;
using Kozma.net.Enums;
using Kozma.net.Factories;
using Kozma.net.Helpers;
using Kozma.net.Models;
using Kozma.net.Trackers;

namespace Kozma.net.Commands.Games;

public class Punch(IEmbedFactory embedFactory, IPunchHelper punchHelper, IPunchTracker punchTracker) : InteractionModuleBase<SocketInteractionContext>
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
        await CraftItemAsync(Context, punchHelper.ConvertToPunchOption(item));
    }

    public async Task CraftItemAsync(SocketInteractionContext context, PunchOption? item, int counter = 1)
    {
        if (item is null)
        {
            await context.Interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embedFactory.GetAndBuildEmbed("Something went wrong while crafting"));
            return;
        }

        var itemData = punchHelper.GetItem((PunchOption)item)!;
        var craftUvs = CraftItem(context.User.Id, itemData);
        var fields = craftUvs.Select((uv, index) => embedFactory.CreateField($"UV #{index + 1}", uv)).ToList();
        fields.Add(embedFactory.CreateField("Crafted", counter.ToString(), inline: false));

        var embed = embedFactory.GetEmbed($"You crafted: {itemData.Name}")
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(itemData.Image)
            .WithFields(fields);
        var components = new ComponentBuilder()
            .WithButton(label: "Recraft", customId: "recraft", style: ButtonStyle.Primary)
            .WithButton(label: "Start Rolling Uvs", customId: "start-punching", style: ButtonStyle.Primary);

        await punchHelper.SendWaitingAnimationAsync(embedFactory.GetEmbed(string.Empty), context, "https://cdn.discordapp.com/attachments/1069643121622777876/1069643186978430996/crafting.gif", 2500);

        await context.Interaction.ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed.Build();
            msg.Components = components.Build();
        });
    }

    private List<string> CraftItem(ulong id, PunchItem item)
    {
        int craftRoll = _random.Next(1, 1001);
        var uvs = new List<string>();

        if (craftRoll == 1) // 1/1000
        {
            for (int i = 0; i < 3; i++)
            {
                uvs.Add(punchHelper.RollUv(id, item, uvs, crafting: true));
            }
        }
        else if (craftRoll <= 11) // 1/100
        {
            for (int i = 0; i < 2; i++)
            {
                uvs.Add(punchHelper.RollUv(id, item, uvs, crafting: true));
            }
        }
        else if (craftRoll <= 111) // 1/10
        {
            uvs.Add(punchHelper.RollUv(id, item, uvs, crafting: true));
        }

        return uvs;
    }
}
