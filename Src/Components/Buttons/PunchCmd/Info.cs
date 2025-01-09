using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Components.Buttons.PunchCmd;

public class Info(IEmbedHandler embedHandler, IPunchHelper punchHelper, IPunchTracker punchTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{ComponentIds.PunchInfoBase}*")]
    public async Task ExecuteAsync(string choice)
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var oldEmbed = context.Message.Embeds.First();
        var fields = oldEmbed.Fields.Select(f => embedHandler.CreateField(f.Name, f.Value, !f.Name.Contains("Crowns", StringComparison.OrdinalIgnoreCase)));
        var uvFields = oldEmbed.Fields.Where(f => f.Name.Contains("UV", StringComparison.OrdinalIgnoreCase)).ToList();
        var lockCount = uvFields.Count(f => f.Name.Contains(Emotes.Locked, StringComparison.OrdinalIgnoreCase));
        var desc = choice switch
        {
            ComponentIds.PunchInfoStats => punchTracker.GetData(Context.User.Id, oldEmbed.Title),
            ComponentIds.PunchInfoOdds => "*These are the chances to get Unique Variants*\n\n" +
            "**When rolling at Punch:**\n- Low: ~ 73.17%\n- Medium: ~ 19.51%\n- High: ~ 4.87%\n- Very High/Maximum: ~ 2.45%\n\n" +
            "**When crafting:**\n- 1/10 for 1 UV\n- 1/100 for 2 UVs\n- 1/1000 for 3 UVs",
            ComponentIds.PunchInfoGamble => "*These buttons let you roll for additional Unique Variants.*",
            ComponentIds.PunchInfoLock => "*These buttons let you lock/unlock a Unique Variant.*",
            _ => "Invalid choice"
        };

        var embed = embedHandler.GetEmbed(oldEmbed.Title)
            .WithDescription(desc)
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
