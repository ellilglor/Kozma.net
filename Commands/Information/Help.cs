using Discord.Interactions;
using Kozma.net.Factories;

namespace Kozma.net.Commands.Information;

public class Help : InteractionModuleBase<SocketInteractionContext>, ICommand
{
    private readonly IEmbedFactory _embedFactory;

    public Help(IEmbedFactory embedFactory)
    {
        _embedFactory = embedFactory;
    }

    [SlashCommand("help", "Explains all commands.")]
    public async Task ExecuteAsync()
    {
        await ModifyOriginalResponseAsync(msg => msg.Embed = _embedFactory.GetAndBuildEmbed("You executed help"));
    }
}
