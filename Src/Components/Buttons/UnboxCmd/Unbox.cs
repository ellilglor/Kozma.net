using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Services;
using Kozma.net.Src.Trackers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Components.Buttons.UnboxCmd;

public class Unbox(IConfiguration config,
    IMemoryCache cache,
    IEmbedHandler embedHandler,
    ICostCalculator costCalculator,
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
                var command = new Commands.Games.Unbox(config, cache, embedHandler, costCalculator, unboxTracker, unboxService, jsonFileReader, logger);
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
