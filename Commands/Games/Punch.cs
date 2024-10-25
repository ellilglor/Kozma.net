﻿using Discord;
using Discord.Interactions;
using Kozma.net.Enums;
using Kozma.net.Factories;
using Kozma.net.Helpers;
using Kozma.net.Models;

namespace Kozma.net.Commands.Games;

public class Punch(IEmbedFactory embedFactory, IboxHelper boxHelper) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("unbox", "Simulate opening a Prize Box or Lockbox.")]
    public async Task ExecuteAsync(
        [Summary(name: "box", description: "Select the box you want to open.")] Box box)
    {
        await UnboxAsync(box);
    }

    public async Task UnboxAsync(Box box)
    {
        var boxData = boxHelper.GetBox(box)!;
        var author = new EmbedAuthorBuilder().WithName(box.ToString()).WithIconUrl(boxData.Url);
        var finalEmbed = embedFactory.GetEmbed("You unboxed:").WithAuthor(author);
        var unboxed = await OpenAsync(box);
        
        if (unboxed.Count == 0)
        {
            await ModifyOriginalResponseAsync(msg => msg.Embed = finalEmbed.WithDescription("Something went wrong while trying to open the box.").Build());
            // TODO: log in case this happens?
            return;
        }

        await SendOpeningAnimationAsync(author, boxData.Gif);

        var url = unboxed.First().Url;
        var description = string.Join(" & ", unboxed.Select(item => item.Name));
        var components = new ComponentBuilder()
            .WithButton(emote: new Emoji("\U0001F501"), customId: "unbox-again", style: ButtonStyle.Secondary)
            .WithButton(emote: new Emoji("\U0001F4D8"), customId: "unbox-stats", style: ButtonStyle.Secondary);

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = finalEmbed.WithDescription($"*{description}*").WithImageUrl(url).Build();
            msg.Components = components.Build();
        });
    }

    private async Task SendOpeningAnimationAsync(EmbedAuthorBuilder author, string url)
    {
        var embed = embedFactory.GetEmbed(string.Empty)
            .WithAuthor(author)
            .WithImageUrl(url)
            .Build();

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = new ComponentBuilder().Build();
        });
        await Task.Delay(3000); // Give the gif time to play
    }

    private async Task<List<ItemData>> OpenAsync(Box box)
    {
        var items = await boxHelper.GetItemDataAsync(box);
        var bonusBoxes = new List<Box>() { Box.Confection, Box.Lucky };
        var unboxed = new List<ItemData>();
        var prevOdds = 0.00;
        var roll = new Random().NextDouble() * 100;

        foreach (var item in items)
        {
            if (prevOdds <= roll && roll < prevOdds + item.Chance)
            {
                unboxed.Add(item);

                /*if (bonusBoxes.Contains(box))
                {
                    var bonusItem = BonusRoll(box, content, roll, item.Name);
                    if (bonusItem != null)
                    {
                        unboxed.Add(bonusItem);
                    }
                }*/

                return unboxed;
            }

            prevOdds += item.Chance;
        }

        return unboxed;
    }
}
