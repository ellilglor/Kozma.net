using Discord.Interactions;
using Kozma.net.Factories;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Commands.Server;

public class TradeEdit : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IEmbedFactory _embedFactory;
    private readonly IConfiguration _config;

    public TradeEdit(IEmbedFactory embedFactory, IConfigFactory configFactory)
    {
        _embedFactory = embedFactory;
        _config = configFactory.GetConfig();
    }

    [SlashCommand("tradepostedit", "Gives you 2 minutes to edit your tradeposts.")]
    public async Task ExecuteAsync()
    {
        var role = Context.Guild.GetRole(_config.GetValue<ulong>("ids:editRoleId"));
        var user = Context.Guild.GetUser(Context.User.Id);
        var embed = _embedFactory.GetEmbed("You have 2 minutes to edit your tradeposts.")
            .WithDescription("Using this command to bypass the slowmode will result in a timeout.");

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
        await user.AddRoleAsync(role);

        // wait 2 minutes
        await Task.Delay(TimeSpan.FromMinutes(2));

        await user.RemoveRoleAsync(role);
        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle("Your time is up!").WithDescription(null).Build());
    }
}
