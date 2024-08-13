using Discord.Interactions;
using Discord;
using Kozma.net.Factories;

namespace Kozma.net.Commands.Server;

public class Test(IEmbedFactory embedFactory) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("test", "Kozma's Backpack staff only.")]
    [RequireUserPermission(GuildPermission.BanMembers | GuildPermission.KickMembers)]
    public async Task ExecuteAsync()
    {
        var embed = embedFactory.GetAndBuildEmbed("Command used for testing.");

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed);
    }
}
