using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Factories;
using Kozma.net.Helpers;

namespace Kozma.net.Components.Buttons.PunchCmd;

public class Lock(IEmbedFactory embedFactory, IPunchHelper punchHelper) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("punch-lock-*")]
    public async Task ExecuteAsync(string number)
    {
        var count = int.Parse(number);
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

            if (i + 1 == count)
            {
                fields.Add(embedFactory.CreateField(field.Name.Contains(locked) ? field.Name.Replace(locked, unlocked) : field.Name.Replace(unlocked, locked), field.Value));
            }
            else
            {
                fields.Add(embedFactory.CreateField(field.Name, field.Value));
            }
        }
        var lockCount = fields.Count(f => f.Name.Contains(locked));
        fields.AddRange(otherFields.Select(field => embedFactory.CreateField(field.Name, field.Value, field.Name != "Crowns Spent")));

        var embed = embedFactory.GetEmbed(oldEmbed.Title)
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(oldEmbed.Thumbnail!.Value.Url)
            .WithFields(fields);

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed.Build();
            msg.Components = punchHelper.GetComponents(uvFields.Count < 1, uvFields.Count < 2, uvFields.Count < 3, lockCount > 0, lockCount > 1, lockCount > 2);
        });
    }
}
