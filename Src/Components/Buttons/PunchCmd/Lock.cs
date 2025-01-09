using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;

namespace Kozma.net.Src.Components.Buttons.PunchCmd;

public class Lock(IEmbedHandler embedHandler, IPunchHelper punchHelper) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{ComponentIds.PunchLock}*")]
    public async Task ExecuteAsync(string number)
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var oldEmbed = context.Message.Embeds.First();
        var uvFields = oldEmbed.Fields.Where(f => f.Name.Contains("UV", StringComparison.OrdinalIgnoreCase)).ToList();
        var otherFields = oldEmbed.Fields.Where(f => !f.Name.Contains("UV", StringComparison.OrdinalIgnoreCase)).ToList();
        var fields = new List<EmbedFieldBuilder>();

        for (int i = 0; i < uvFields.Count; i++)
        {
            var field = uvFields[i];

            if (i + 1 == int.Parse(number))
            {
                fields.Add(embedHandler.CreateField(field.Name.Contains(Emotes.Locked, StringComparison.OrdinalIgnoreCase) ? field.Name.Replace(Emotes.Locked, Emotes.Unlocked, StringComparison.OrdinalIgnoreCase) : field.Name.Replace(Emotes.Unlocked, Emotes.Locked, StringComparison.OrdinalIgnoreCase), field.Value));
            }
            else
            {
                fields.Add(embedHandler.CreateField(field.Name, field.Value));
            }
        }
        var lockCount = fields.Count(f => f.Name.Contains(Emotes.Locked, StringComparison.OrdinalIgnoreCase));
        fields.AddRange(otherFields.Select(field => embedHandler.CreateField(field.Name, field.Value, field.Name != "Crowns Spent")));

        var embed = embedHandler.GetEmbed(oldEmbed.Title)
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(oldEmbed.Thumbnail!.Value.Url)
            .WithFields(fields);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed.Build();
            msg.Components = punchHelper.GetComponents(uvFields.Count, lockCount);
        });
    }
}
