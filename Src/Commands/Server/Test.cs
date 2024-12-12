using Discord;
using Discord.Interactions;
using Kozma.net.Src.Handlers;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
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
