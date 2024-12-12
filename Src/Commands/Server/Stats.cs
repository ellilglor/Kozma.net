using Discord.Interactions;
using Discord;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
public class Stats(IStatPageTracker pageTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("stats", "Kozma's Backpack staff only.")]
    [RequireUserPermission(GuildPermission.BanMembers | GuildPermission.KickMembers)]
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
