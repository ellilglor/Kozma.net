using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Models;
using Kozma.net.Src.Services;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kozma.net.Components.Buttons.PunchCmd;

public partial class Roll(IEmbedHandler embedHandler, IPunchHelper punchHelper, IPunchService punchService) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("punch-gamble-*")]
    public async Task ExecuteAsync(string number)
    {
        var count = int.Parse(number);
        var context = (SocketMessageComponent)Context.Interaction;
        var oldEmbed = context.Message.Embeds.First();
        var itemData = oldEmbed.Title.ConvertToPunchOption().ToPunchItem();
        var uvFields = oldEmbed.Fields.Where(f => f.Name.Contains("uv", StringComparison.OrdinalIgnoreCase)).ToList();
        var lockCount = uvFields.Count(f => f.Name.Contains(Emotes.Locked, StringComparison.OrdinalIgnoreCase));
        var cost = count == 1 ? PunchPrices.SingleTicket : count == 2 ? PunchPrices.DoubleTicket : PunchPrices.TripleTicket;

        var embed = await BuildEmbedAsync(itemData, count, cost, uvFields, oldEmbed.Fields);

        await punchService.UpdateOrSaveGamblerAsync(Context.User.Id, Context.User.Username, cost);
        await punchHelper.SendWaitingAnimationAsync(embedHandler.GetEmbed(string.Empty), Context.Interaction, itemData.Gif, 1500);

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = punchHelper.GetComponents(count, lockCount);
        });
    }

    private async Task<Embed> BuildEmbedAsync(PunchItem itemData, int uvCount, PunchPrices cost, IReadOnlyCollection<EmbedField> uvFields, ImmutableArray<EmbedField> oldFields)
    {
        var fields = new List<EmbedFieldBuilder>();

        var uvs = RollForUvs(uvCount, uvFields, itemData, Context.User.Id);
        foreach (var uv in uvs)
        {
            var index = uv.IndexOf(':', StringComparison.InvariantCulture);
            fields.Add(embedHandler.CreateField(uv[..index], uv[(index + 1)..]));
        }

        var spent = int.Parse(oldFields.FirstOrDefault(f => f.Name.Contains("Crowns Spent", StringComparison.OrdinalIgnoreCase)).Value, NumberStyles.AllowThousands, CultureInfo.CurrentCulture);
        fields.Add(embedHandler.CreateField("Crowns Spent", $"{(spent + (int)cost).ToString("N0", CultureInfo.CurrentCulture)}", isInline: false));
        UpdateRollCounter(oldFields, uvCount, fields);

        var (desc, image) = await punchHelper.CheckForGmAsync(Context.User.Username, itemData.Type, uvs);
        var embed = embedHandler.GetEmbed(itemData.Name)
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(itemData.Image)
            .WithDescription(desc)
            .WithImageUrl(image)
            .WithFields(fields);

        return embed.Build();
    }

    private List<string> RollForUvs(int count, IReadOnlyCollection<EmbedField> uvFields, PunchItem item, ulong id)
    {
        var uvs = uvFields
            .Where(f => f.Name.Contains(Emotes.Locked, StringComparison.OrdinalIgnoreCase))
            .Select((uv, index) => $"{uv.Name.Replace(NumRegex().Match(uv.Name).Value, (index + 1).ToString(), StringComparison.InvariantCulture)}:{uv.Value}")
            .ToList();

        for (int i = uvs.Count; i < count; i++)
        {
            uvs.Add($"{Emotes.Unlocked} UV #{i + 1}:{punchHelper.RollUv(id, item, uvs)}");
        }

        return uvs;
    }

    private void UpdateRollCounter(IImmutableList<EmbedField> countFields, int count, List<EmbedFieldBuilder> fields)
    {
        var singleField = countFields.FirstOrDefault(f => f.Name == "Single Rolls");
        var doubleField = countFields.FirstOrDefault(f => f.Name == "Double Rolls");
        var tripleField = countFields.FirstOrDefault(f => f.Name == "Triple Rolls");

        switch (count)
        {
            case 1:
                fields.Add(embedHandler.CreateField("Single Rolls", singleField.Name is null ? "1" : (int.Parse(singleField.Value) + 1).ToString()));
                if (doubleField.Name != null) fields.Add(embedHandler.CreateField(doubleField.Name, doubleField.Value));
                if (tripleField.Name != null) fields.Add(embedHandler.CreateField(tripleField.Name, tripleField.Value));
                break;
            case 2:
                if (singleField.Name != null) fields.Add(embedHandler.CreateField(singleField.Name, singleField.Value));
                fields.Add(embedHandler.CreateField("Double Rolls", doubleField.Name is null ? "1" : (int.Parse(doubleField.Value) + 1).ToString()));
                if (tripleField.Name != null) fields.Add(embedHandler.CreateField(tripleField.Name, tripleField.Value));
                break;
            case 3:
                if (singleField.Name != null) fields.Add(embedHandler.CreateField(singleField.Name, singleField.Value));
                if (doubleField.Name != null) fields.Add(embedHandler.CreateField(doubleField.Name, doubleField.Value));
                fields.Add(embedHandler.CreateField("Triple Rolls", tripleField.Name is null ? "1" : (int.Parse(tripleField.Value) + 1).ToString()));
                break;
        }
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumRegex();
}
