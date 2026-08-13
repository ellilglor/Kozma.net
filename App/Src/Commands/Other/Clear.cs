using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Kozma.net.Src.Data.Constants;
using Kozma.net.Src.Interfaces.Handlers;

namespace Kozma.net.Src.Commands.Other;

public class Clear(IEmbedHandler embedHandler, IRateLimitHandler rateLimitHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand(CommandIds.Clear, "Removes all bot messages in your dms.")]
    [ComponentInteraction(ComponentIds.ClearMessages)]
    public async Task ExecuteAsync()
    {
        await RespondAsync(Context.Interaction);
        await ClearMessagesAsync(Context.User);
    }

    private async Task RespondAsync(SocketInteraction interaction)
    {
        try
        {
            await interaction.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embedHandler.GetAndBuildEmbed("Clearing messages.");
                msg.Components = new ComponentBuilder().Build();
            });
        }
        catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.UnknownMessage)
        {
            return;
        }
    }

    private async Task ClearMessagesAsync(SocketUser user)
    {
        var channel = await user.CreateDMChannelAsync();
        var messages = await channel.GetMessagesAsync(int.MaxValue).FlattenAsync();

        foreach (var msg in messages.Where(msg => msg.Author.IsBot))
        {
            while (rateLimitHandler.IsRateLimited())
                await Task.Delay(500);

            try
            {
                await msg.DeleteAsync();
            }
            catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.UnknownMessage || e.DiscordCode == DiscordErrorCode.CannotExecuteForDM)
            {
                continue; // Can happen if /clear gets run twice before the first one has finished or a random "cannot execute action on a DM channel"
            }

            await Task.Delay(420);
        }
    }
}
