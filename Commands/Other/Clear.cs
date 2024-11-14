using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Handlers;

namespace Kozma.net.Commands.Other;

public class Clear(IEmbedHandler embedHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("clear", "Removes all bot messages in your dms.")]
    public async Task ExecuteAsync()
    {
        await RespondAsync(Context.Interaction);
        await ClearMessagesAsync(Context.User);
    }

    public async Task RespondAsync(SocketInteraction interaction)
    {
        var embed = embedHandler.GetAndBuildEmbed("Clearing messages.");

        await interaction.ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = new ComponentBuilder().Build();
        });
    }

    public async Task ClearMessagesAsync(SocketUser user)
    {
        var channel = await user.CreateDMChannelAsync();
        var messages = await channel.GetMessagesAsync(int.MaxValue).FlattenAsync();

        foreach (var msg in messages.Where(msg => msg.Author.IsBot))
        {
            try
            {
                await msg.DeleteAsync();
                await Task.Delay(700); // delay to prevent hitting Discord rate limit
            }
            catch { }
        }
    }
}
