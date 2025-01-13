using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
[DefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.KickMembers | GuildPermission.BanMembers)]
public class PriceCheck(IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand(CommandIds.PriceCheck, "Kozma's Backpack staff only.")]
    public async Task ExecuteAsync()
    {
        await Context.Channel.SendFileAsync(
            filePath: Path.Combine("Src", "Assets", "we-dont-do-that-here.jpg"),
            text: $"Asking for prices outside of {MentionUtils.MentionChannel(config.GetValue<ulong>("ids:priceCheckChannel"))}?"
        );

        await ModifyOriginalResponseAsync(msg => msg.Content = "Image posted.");
    }
}
