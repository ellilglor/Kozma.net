using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
[DefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.KickMembers | GuildPermission.BanMembers)]
public class Stats(IStatPageTracker pageTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand(CommandIds.Stats, "Kozma's Backpack staff only.")]
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
