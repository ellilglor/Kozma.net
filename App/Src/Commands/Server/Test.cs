using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
[DefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.KickMembers | GuildPermission.BanMembers)]
public class Test(IEmbedHandler embedHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand(CommandIds.Test, "Kozma's Backpack staff only.")]
    public async Task ExecuteAsync()
    {
        var embed = embedHandler.GetAndBuildEmbed("Command used for testing.");

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed);
    }
}
