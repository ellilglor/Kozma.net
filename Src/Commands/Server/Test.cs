using Discord;
using Discord.Interactions;
using Kozma.net.Src.Handlers;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
[RequireUserPermission(GuildPermission.Administrator | GuildPermission.KickMembers | GuildPermission.BanMembers, Group = "Permission")]
[RequireOwner(Group = "Permission")]
public class Test(IEmbedHandler embedHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("test", "Kozma's Backpack staff only.")]
    public async Task ExecuteAsync()
    {
        var embed = embedHandler.GetAndBuildEmbed("Command used for testing.");

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed);
    }
}
