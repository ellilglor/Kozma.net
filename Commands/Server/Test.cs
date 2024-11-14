using Discord.Interactions;
using Discord;
using Kozma.net.Handlers;

namespace Kozma.net.Commands.Server;

public class Test(IEmbedHandler embedHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("test", "Kozma's Backpack staff only.")]
    [RequireUserPermission(GuildPermission.BanMembers | GuildPermission.KickMembers)]
    public async Task ExecuteAsync()
    {
        var embed = embedHandler.GetAndBuildEmbed("Command used for testing.");

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed);
    }
}
