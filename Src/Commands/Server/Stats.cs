using Discord;
using Discord.Interactions;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
[RequireUserPermission(GuildPermission.Administrator | GuildPermission.KickMembers | GuildPermission.BanMembers, Group = "Permission")]
[RequireOwner(Group = "Permission")]
public class Stats(IStatPageTracker pageTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("stats", "Kozma's Backpack staff only.")]
    public async Task ExecuteAsync()
    {
        await pageTracker.BuildPagesAsync();

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = pageTracker.GetPage(Context.User.Id);
            msg.Components = pageTracker.GetComponents(Context.User.Id);
        });
    }
}
