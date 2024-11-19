using Discord;
using Discord.Interactions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Commands.Information;

public class Help(IEmbedHandler embedHandler, IConfiguration config, IFileReader jsonFileReader) : InteractionModuleBase<SocketInteractionContext>
{
    private record CommandInfo(string Command, string Description);

    [SlashCommand("help", "Explains all commands.")]
    public async Task ExecuteAsync()
    {
        var info = await jsonFileReader.ReadAsync<IEnumerable<CommandInfo>>(Path.Combine("Data", "Help", "Commands.json")) ?? [];
        var fields = info.Select(cmd => embedHandler.CreateField(cmd.Command, cmd.Description, inline: false)).ToList();

        var embed = embedHandler.GetEmbed("Here are all my commands:")
            .WithDescription($"*If you notice a problem please contact <@{config.GetValue<string>("ids:owner")}>*")
            .WithFields(fields)
            .Build();

        var components = new ComponentBuilder()
            .WithButton(label: "Github", url: config.GetValue<string>("github"), style: ButtonStyle.Link)
            .WithButton(label: "Discord server", url: config.GetValue<string>("serverInvite"), style: ButtonStyle.Link)
            .Build();

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed;
            msg.Components = components;
        });
    }
}
