using Discord;
using Discord.Interactions;

namespace Kozma.net.Commands.Server;

public class PriceCheck : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("pricecheck", "Kozma's Backpack staff only.")]
    [RequireUserPermission(GuildPermission.BanMembers | GuildPermission.KickMembers)]
    public async Task ExecuteAsync()
    {
        var projectRoot = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;

        if (projectRoot == null)
        {
            await ModifyOriginalResponseAsync(msg => msg.Content = "Failed to find project root.");
        }
        else
        {
            await Context.Channel.SendFileAsync(
                filePath: Path.Combine(projectRoot, "Assets", "we-dont-do-that-here.jpg"),
                text: "Asking for prices outside of <#1022505768869711963>?"
            );

            await ModifyOriginalResponseAsync(msg => msg.Content = "Image posted.");
        }
    }
}
