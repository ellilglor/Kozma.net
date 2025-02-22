using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;

namespace Kozma.net.Src.Components.Buttons.ShardSweepCmd;

public class Info(IEmbedHandler embedHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction(ComponentIds.ShardSweepInfo)]
    public async Task ExecuteAsync()
    {
        var context = (SocketMessageComponent)Context.Interaction;

        if (context.Message.Embeds.Count > 0)
        {
            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = context.Message.Content;
                msg.Embed = null;
            });
        }
        else
        {
            var embed = embedHandler.GetEmbed("How to play Shardsweeper")
            .WithImageUrl("https://cdn.discordapp.com/attachments/1069643121622777876/1326920912209186940/image.png?ex=67812ee6&is=677fdd66&hm=f30a725fb8a23b699e7a4d91fd8509b83840973ac397b969f2bfa76bac021e06&")
            .WithDescription($"The playing field consists of a 9x9 area for a total of {Format.Bold("81")} squares.\n" +
                $"The goal is to unspoiler all squares that do not contain a {Emotes.Shard}.\n" +
                $"There are a total of {Format.Bold("10")} {Emotes.Shard} scattered across the playing field.\n\n" +
                $"Squares indicated by a {Emotes.Square} do not touch a {Emotes.Shard}.\n" +
                $"Those indicated by a number like {Emotes.One} are surrounded by that amount of shards.\n\nExample field:");

            await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
        }
    }
}
