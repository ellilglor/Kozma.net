using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;

namespace Kozma.net.Src.Commands.Information;

public class ServerPortal(IEmbedHandler embedHandler, IFileReader jsonFileReader) : InteractionModuleBase<SocketInteractionContext>
{
    private sealed record ServerInfo(string Name, string Invite, string Description);

    [SlashCommand(CommandIds.ServerPortal, "Gives a list of community servers.")]
    public async Task ExecuteAsync()
    {
        var info = await jsonFileReader.ReadAsync<IEnumerable<ServerInfo>>(Path.Combine("Data", "Servers.json"));
        var desc = string.Join("\n", info.Select(server => $"{Format.Header(Format.Url(server.Name, server.Invite), level: 3)}\n{server.Description}"));

        var embed = embedHandler.GetEmbed(string.Empty)
            .WithDescription(desc);

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
    }
}
