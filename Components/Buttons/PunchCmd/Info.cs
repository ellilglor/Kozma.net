using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Factories;
using Kozma.net.Helpers;
using Kozma.net.Trackers;

namespace Kozma.net.Components.Buttons.PunchCmd;

public class Info(IEmbedFactory embedFactory, IPunchHelper punchHelper, IPunchTracker punchTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("punch-info-*")]
    public async Task ExecuteAsync(string choice)
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var oldEmbed = context.Message.Embeds.First();
        var fields = oldEmbed.Fields.Select(f => embedFactory.CreateField(f.Name, f.Value, !f.Name.Contains("Crowns")));
        var uvFields = oldEmbed.Fields.Where(f => f.Name.Contains("UV")).ToList();
        var lockCount = uvFields.Count(f => f.Name.Contains("\U0001f512"));
        var desc = choice switch
        {
            "stats" => punchTracker.GetData(Context.User.Id, oldEmbed.Title),
            "odds" => "*These are the chances to get Unique Variants*\n\n" +
            "**When rolling at Punch:**\n- Low: ~ 73.17%\n- Medium: ~ 19.51%\n- High: ~ 4.87%\n- Very High/Maximum: ~ 2.45%\n\n" +
            "**When crafting:**\n- 1/10 for 1 UV\n- 1/100 for 2 UVs\n- 1/1000 for 3 UVs",
            "gamble" => "*These buttons let you roll for additional Unique Variants.*",
            "lock" => "*These buttons let you lock/unlock a Unique Variant.*",
            _ => "Invalid choice"
        };

        var embed = embedFactory.GetEmbed(oldEmbed.Title)
            .WithDescription(desc)
            .WithAuthor(punchHelper.GetAuthor())
            .WithThumbnailUrl(oldEmbed.Thumbnail!.Value.Url)
            .WithFields(fields);

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed.Build();
            msg.Components = punchHelper.GetComponents(uvFields.Count < 1, uvFields.Count < 2, uvFields.Count < 3, lockCount > 0, lockCount > 1, lockCount > 2);
        });
    }
}
