using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;

namespace Kozma.net.Src.Components.Buttons.PunchCmd;

public class Lock(IEmbedHandler embedHandler, IPunchHelper punchHelper) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("punch-lock-*")]
    public async Task ExecuteAsync(string number)
    {
        var position = int.Parse(number);
        var context = (SocketMessageComponent)Context.Interaction;
        var oldEmbed = context.Message.Embeds.First();
        var uvFields = oldEmbed.Fields.Where(f => f.Name.Contains("UV")).ToList();
        var otherFields = oldEmbed.Fields.Where(f => !f.Name.Contains("UV")).ToList();
        var fields = new List<EmbedFieldBuilder>();
        var locked = "\U0001f512";
        var unlocked = "\U0001f513";

        for (int i = 0; i < uvFields.Count; i++)
        {
            var field = uvFields[i];

            if (i + 1 == position)
            {
                fields.Add(embedHandler.CreateField(field.Name.Contains(locked) ? field.Name.Replace(locked, unlocked) : field.Name.Replace(unlocked, locked), field.Value));
            }
            else
            {
                fields.Add(embedHandler.CreateField(field.Name, field.Value));
            }
        }
        var lockCount = fields.Count(f => f.Name.Contains(locked));
        fields.AddRange(otherFields.Select(field => embedHandler.CreateField(field.Name, field.Value, field.Name != "Crowns Spent")));

        var embed = embedHandler.GetEmbed(oldEmbed.Title)
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(oldEmbed.Thumbnail!.Value.Url)
            .WithFields(fields);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed.Build();
            msg.Components = punchHelper.GetComponents(uvFields.Count < 1, uvFields.Count < 2, uvFields.Count < 3, lockCount > 0, lockCount > 1, lockCount > 2);
        });
    }
}
