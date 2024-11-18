using Discord.Interactions;
using Discord;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Commands.Server;

public class Stats(IStatPageTracker pageTracker) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("stats", "Kozma's Backpack staff only.")]
    [RequireUserPermission(GuildPermission.BanMembers | GuildPermission.KickMembers)]
    public async Task ExecuteAsync()
    {
        await pageTracker.BuildPagesAsync();

        var embed = pageTracker.GetPage(Context.User.Id);
        var components = pageTracker.GetComponents(Context.User.Id);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed;
            msg.Components = components;
        });
    }
}
