using Discord;
using Discord.Interactions;
using Kozma.net.Enums;
using Kozma.net.Factories;
using Kozma.net.Helpers;

namespace Kozma.net.Commands.Games;

public class Punch(IEmbedFactory embedFactory, IPunchHelper punchHelper) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Random _random = new();

    [SlashCommand("punch", "Craft and roll on an item for Unique Variants.")]
    public async Task ExecuteAsync(
        [Summary(name: "item", description: "Select the item you want to craft.")] PunchOption item)
    {
        await CraftItemAsync(Context, item);
    }

    public async Task CraftItemAsync(SocketInteractionContext context, PunchOption item, int counter = 1)
    {
        var itemData = punchHelper.GetItem(item)!;
        var craftUvs = CraftItem(itemData.Type);
        var fields = new List<EmbedFieldBuilder>();

        for (int i = 0; i < craftUvs.Count; i++)
        {
            fields.Add(embedFactory.CreateField($"UV #{i + 1}", craftUvs[i]));
        }
        fields.Add(embedFactory.CreateField("Crafted", counter.ToString(), inline: false));

        var embed = embedFactory.GetEmbed($"You crafted: {item}")
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(itemData.Image)
            .WithFields(fields);
        var components = new ComponentBuilder()
            .WithButton(label: "Recraft", customId: "recraft", style: ButtonStyle.Primary)
            .WithButton(label: "Start Rolling Uvs", customId: "start-punching", style: ButtonStyle.Primary);

        await SendWaitingAnimationAsync(context, "https://cdn.discordapp.com/attachments/1069643121622777876/1069643186978430996/crafting.gif", 2500);

        await context.Interaction.ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed.Build();
            msg.Components = components.Build();
        });
    }

    private async Task SendWaitingAnimationAsync(SocketInteractionContext context, string url, int delay)
    {
        var embed = embedFactory.GetEmbed(string.Empty)
            .WithAuthor(punchHelper.GetAuthor())
            .WithImageUrl(url)
            .Build();

        await context.Interaction.ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = new ComponentBuilder().Build();
        });
        await Task.Delay(delay); // Give the gif time to play
    }

    private List<string> CraftItem(ItemType type)
    {
        int craftRoll = new Random().Next(1, 1001);
        var uvs = new List<string>();

        if (craftRoll == 1) // 1/1000
        {
            for (int i = 0; i < 3; i++)
            {
                uvs.Add(punchHelper.RollUv(type, uvs, crafting: true));
            }
        }
        else if (craftRoll <= 11) // 1/100
        {
            for (int i = 0; i < 2; i++)
            {
                uvs.Add(punchHelper.RollUv(type, uvs, crafting: true));
            }
        }
        else if (craftRoll <= 111) // 1/10
        {
            uvs.Add(punchHelper.RollUv(type, uvs, crafting: true));
        }

        return uvs;
    }
}
