using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Handlers;

namespace Kozma.net.Src.Commands.Other;

public class Clear(IEmbedHandler embedHandler, IRateLimitHandler rateLimitHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("clear", "Removes all bot messages in your dms.")]
    [ComponentInteraction("clear-messages")]
    public async Task ExecuteAsync()
    {
        await RespondAsync(Context.Interaction);
        await ClearMessagesAsync(Context.User);
    }

    private async Task RespondAsync(SocketInteraction interaction)
    {
        await interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embedHandler.GetAndBuildEmbed("Clearing messages.");
            msg.Components = new ComponentBuilder().Build();
        });
    }

    private async Task ClearMessagesAsync(SocketUser user)
    {
        var channel = await user.CreateDMChannelAsync();
        var messages = await channel.GetMessagesAsync(int.MaxValue).FlattenAsync();

        foreach (var msg in messages.Where(msg => msg.Author.IsBot))
        {
            while (rateLimitHandler.IsRateLimited())
            {
                await Task.Delay(500);
            }

            try
            {
                await msg.DeleteAsync();
            }
            catch (Exception e) when (e.Message.Contains("10008", StringComparison.InvariantCulture))
            {
                continue; // Can happen if /clear gets run twice before the first one has finished
            }

            await Task.Delay(420);
        }
    }
}
