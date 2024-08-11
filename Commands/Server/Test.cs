using Discord.Interactions;
using Discord;
using Kozma.net.Factories;

namespace Kozma.net.Commands.Server;

public class Test : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IEmbedFactory _embedFactory;

    public Test(IEmbedFactory embedFactory)
    {
        _embedFactory = embedFactory;
    }

    [SlashCommand("test", "Kozma's Backpack staff only.")]
    [RequireUserPermission(GuildPermission.BanMembers | GuildPermission.KickMembers)]
    public async Task ExecuteAsync()
    {
        var embed = _embedFactory.GetAndBuildEmbed("Command used for testing.");

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed);
    }
}
