using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Factories;

namespace Kozma.net.Commands.Other;

public class Clear(IEmbedFactory embedFactory) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("clear", "Removes all bot messages in your dms.")]
    public async Task ExecuteAsync()
    {
        var embed = embedFactory.GetAndBuildEmbed("Clearing messages.");

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = new ComponentBuilder().Build();
        });

        await ClearMessagesAsync(Context.User);
    }

    public static async Task ClearMessagesAsync(SocketUser user)
    {
        var channel = await user.CreateDMChannelAsync();
        var messages = await channel.GetMessagesAsync(int.MaxValue).FlattenAsync();

        foreach (var msg in messages)
        {
            try
            {
                if (msg.Author.IsBot) await msg.DeleteAsync();
            }
            catch { }
        }
    }
}
