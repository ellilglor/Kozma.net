using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
public class TradeEdit(IConfiguration config, IEmbedHandler embedHandler, IRoleHandler roleHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand(CommandIds.TradeEdit, "Gives you 2 minutes to edit your tradeposts.")]
    public async Task ExecuteAsync()
    {
        var user = Context.Guild.GetUser(Context.User.Id);
        var embed = embedHandler.GetEmbed("You have 2 minutes to edit your tradeposts.")
            .WithDescription("Using this command to bypass the slowmode will result in a timeout.");

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
        await roleHandler.GiveRoleAsync(user, config.GetValue<ulong>("ids:roles:edit"));

        // wait 2 minutes
        await Task.Delay(TimeSpan.FromMinutes(2));

        await roleHandler.RemoveRoleAsync(user, config.GetValue<ulong>("ids:roles:edit"));
        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle("Your time is up!").WithDescription(null).Build());
    }
}
