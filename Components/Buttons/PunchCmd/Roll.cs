using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Enums;
using Kozma.net.Handlers;
using Kozma.net.Helpers;
using Kozma.net.Models;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kozma.net.Components.Buttons.PunchCmd;

public partial class Roll(IEmbedHandler embedHandler, IPunchHelper punchHelper) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("punch-gamble-*")]
    public async Task ExecuteAsync(string number)
    {
        var count = int.Parse(number);
        var context = (SocketMessageComponent)Context.Interaction;
        var oldEmbed = context.Message.Embeds.First();
        var itemData = punchHelper.GetItem((PunchOption)punchHelper.ConvertToPunchOption(oldEmbed.Title)!)!;
        var uvFields = oldEmbed.Fields.Where(f => f.Name.Contains("UV")).ToList();
        var lockCount = uvFields.Count(f => f.Name.Contains("\U0001f512"));
        var fields = new List<EmbedFieldBuilder>();

        var uvs = RollForUvs(count, uvFields, itemData, Context.User.Id);
        foreach (var uv in uvs)
        {
            var index = uv.IndexOf(':');
            fields.Add(embedHandler.CreateField(uv[..index], uv[(index + 1)..]));
        }

        var spent = int.Parse(oldEmbed.Fields.FirstOrDefault(f => f.Name.Contains("Crowns Spent")).Value.Replace(".", ","), NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
        var cost = count == 1 ? PunchPrices.Single : count == 2 ? PunchPrices.Double : PunchPrices.Triple;
        fields.Add(embedHandler.CreateField("Crowns Spent", $"{spent + (int)cost:N0}", inline: false));
        UpdateRollCounter(oldEmbed.Fields, count, fields);

        var (desc, image) = await punchHelper.CheckForGmAsync(itemData.Type, uvs);
        var embed = embedHandler.GetEmbed(itemData.Name)
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(itemData.Image)
            .WithDescription(desc)
            .WithImageUrl(image)
            .WithFields(fields);

        await punchHelper.SendWaitingAnimationAsync(embedHandler.GetEmbed(string.Empty), Context.Interaction, itemData.Gif, 1500);

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed.Build();
            msg.Components = punchHelper.GetComponents(count < 1, count < 2, count < 3, lockCount > 0, lockCount > 1, lockCount > 2);
        });
    }

    private List<string> RollForUvs(int count, List<EmbedField> uvFields, PunchItem item, ulong id)
    {
        var uvs = uvFields
            .Where(f => f.Name.Contains("\U0001f512"))
            .Select((uv, index) => $"{uv.Name.Replace(NumRegex().Match(uv.Name).Value, (index + 1).ToString())}:{uv.Value}")
            .ToList();

        for (int i = uvs.Count; i < count; i++)
        {
            uvs.Add($"\U0001f513 UV #{i + 1}:{punchHelper.RollUv(id, item, uvs)}");
        }

        return uvs;
    }

    private void UpdateRollCounter(IImmutableList<EmbedField> countFields, int count, List<EmbedFieldBuilder> fields)
    {
        var singleField = countFields.FirstOrDefault(f => f.Name.Equals("Single Rolls"));
        var doubleField = countFields.FirstOrDefault(f => f.Name.Equals("Double Rolls"));
        var tripleField = countFields.FirstOrDefault(f => f.Name.Equals("Triple Rolls"));

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
