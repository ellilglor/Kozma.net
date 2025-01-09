using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;

namespace Kozma.net.Src.Components.Buttons.ShardSweepCmd;

public class Info(IEmbedHandler embedHandler) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction(ComponentIds.ShardSweepInfo)]
    public async Task ExecuteAsync()
    {
        var embed = embedHandler.GetEmbed("How to play Shardsweeper")
            .WithImageUrl("https://cdn.discordapp.com/attachments/1069643121622777876/1326914676021399694/image.png?ex=67812917&is=677fd797&hm=4c056ad42858e44ce2f2d76fd1301dcc89f5045476c88ee87eb9934e7f27ca6f&")
            .WithDescription("The playing field consists of a 9x9 area for a total of **81** squares." +
            $"\nThe goal is to unspoiler all squares that do not contain a {Emotes.Shard}." +
            $"\nThere are a total of **10** {Emotes.Shard} scattered across the playing field." +
            $"\n\nSquares indicated by a {Emotes.Square} do not touch a {Emotes.Shard}" +
            $"\nThose indicated by a number like {Emotes.One} are surrounded by that amount of shards." +
            "\n\nExample field:");

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
    }
}
