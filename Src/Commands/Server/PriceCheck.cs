using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
[DefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.KickMembers | GuildPermission.BanMembers)]
public class PriceCheck(IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("pricecheck", "Kozma's Backpack staff only.")]
    public async Task ExecuteAsync()
    {
        await Context.Channel.SendFileAsync(
            filePath: Path.Combine("Src", "Assets", "we-dont-do-that-here.jpg"),
            text: $"Asking for prices outside of <#{config.GetValue<string>("ids:priceCheckChannel")}>?"
        );

        await ModifyOriginalResponseAsync(msg => msg.Content = "Image posted.");
    }
}
