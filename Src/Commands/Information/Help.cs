using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Commands.Information;

public class Help(IEmbedHandler embedHandler, IConfiguration config, IFileReader jsonFileReader) : InteractionModuleBase<SocketInteractionContext>
{
    private sealed record CommandInfo(string Command, string Description);

    [SlashCommand(CommandIds.Help, "Explains all commands.")]
    public async Task ExecuteAsync()
    {
        var info = await jsonFileReader.ReadAsync<IEnumerable<CommandInfo>>(Path.Combine("Data", "Help.json"));

        var embed = embedHandler.GetEmbed("Here are all my commands:")
            .WithDescription($"*If you notice a problem please contact <@{config.GetValue<string>("ids:owner")}>*")
            .WithFields(info.Select(cmd => embedHandler.CreateField(cmd.Command, cmd.Description, isInline: false)).ToList());

        var components = new ComponentBuilder()
            .WithButton(label: "Github", url: config.GetValue<string>("github"), style: ButtonStyle.Link)
            .WithButton(label: "Discord server", url: config.GetValue<string>("serverInvite"), style: ButtonStyle.Link);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed.Build();
            msg.Components = components.Build();
        });
    }
}
