using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Constants;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Interfaces;
using Kozma.net.Src.Interfaces.Handlers;
using Kozma.net.Src.Interfaces.Helpers;
using Kozma.net.Src.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Kozma.net.Src.Components.Buttons.UnboxCmd;

public class Unbox(IMemoryCache cache,
    IEmbedHandler embedHandler,
    IUnboxTracker unboxTracker,
    IUnboxService unboxService,
    IFileReader jsonFileReader,
    IBotLogger logger) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction($"{ComponentIds.UnboxBase}*")]
    public async Task ExecuteAsync(string action)
    {
        var context = (SocketMessageComponent)Context.Interaction;
        var embed = context.Message.Embeds.First();

        if (Enum.TryParse(embed.Author!.Value.Name, out Box box))
        {
            if (action == ComponentIds.UnboxAgain)
            {
                var command = new Commands.Games.Unbox(cache, embedHandler, unboxTracker, unboxService, jsonFileReader, logger);
                await command.UnboxAsync(Context.Interaction, Context.User.Id, box, int.Parse(embed.Fields[0].Value) + 1);
            }
            else
            {
                await DisplayStatsAsync(embed, box);
            }
        }
        else
        {
            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embedHandler.GetAndBuildEmbed($"Something went wrong while trying to get the box from {box}"); ;
                msg.Components = new ComponentBuilder().Build();
            });
        }
    }

    private async Task DisplayStatsAsync(Embed embed, Box box)
    {
        var boxData = box.ToBoxData();
        var fields = new List<EmbedFieldBuilder>
        {
            embedHandler.CreateField(embed.Fields[0].Name, embed.Fields[0].Value),
            embedHandler.CreateField(embed.Fields[1].Name, embed.Fields[1].Value),
            embedHandler.CreateEmptyField(),
            embedHandler.CreateField("Unique", $"{unboxTracker.GetItemCount(Context.User.Id, box)}"),
            embedHandler.CreateField("Info", $"[Link]({boxData.Page} 'page with distribution of probabilities')"),
            embedHandler.CreateEmptyField()
        };

        var statEmbed = embedHandler.GetEmbed("In this session you opened:")
            .WithAuthor(new EmbedAuthorBuilder().WithName(box.ToString()).WithIconUrl(boxData.Url))
            .WithDescription(unboxTracker.GetData(Context.User.Id, box))
            .WithFields(fields);

        await ModifyOriginalResponseAsync(msg => msg.Embed = statEmbed.Build());
    }
}
