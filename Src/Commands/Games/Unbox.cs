using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Models;
using Kozma.net.Src.Services;
using Kozma.net.Src.Trackers;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Commands.Games;

public class Unbox(IConfiguration config,
    IEmbedHandler embedHandler,
    ICostCalculator costCalculator,
    IUnboxTracker unboxTracker,
    IUnboxService unboxService,
    IFileReader jsonFileReader,
    IBotLogger logger) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Random _random = new();

    [SlashCommand("unbox", "Simulate opening a Prize Box or Lockbox.")]
    public async Task ExecuteAsync(
        [Summary(name: "box", description: "Select the box you want to open.")] Box box)
    {
        unboxTracker.SetPlayer(Context.User.Id, box);
        await UnboxAsync(Context.Interaction, Context.User.Id, box);
    }

    public async Task UnboxAsync(SocketInteraction interaction, ulong userId, Box box, int opened = 1)
    {
        var boxData = box.ToBoxData();
        var author = new EmbedAuthorBuilder().WithName(box.ToString()).WithIconUrl(boxData.Url);
        var cost = costCalculator.CalculateBoxCost(opened, boxData);
        var fields = new List<EmbedFieldBuilder>
        {
            embedHandler.CreateField("Opened", opened.ToString()),
            embedHandler.CreateField("Spent", boxData.Currency.Equals(BoxCurrency.Dollar) ? $"${cost:N2}" : $"{cost:N0} Energy")
        };
        var embed = embedHandler.GetEmbed("You unboxed:").WithAuthor(author).WithFields(fields);
        var unboxed = await OpenAsync(box);

        if (unboxed.Count == 0)
        {
            await interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithDescription("Something went wrong while trying to open the box.").Build());
            throw new ArgumentOutOfRangeException($"Something went wrong while trying to open {box}.");
        }

        var items = string.Join(" & ", unboxed.Select(item => item.Name));
        logger.Log(items.Contains('*', StringComparison.InvariantCulture) ? LogLevel.Special : LogLevel.Info, $"{interaction.User.Username} opened {box} and got {items}");

        await SaveUnboxedAsync(userId, box, unboxed);

        embed.WithDescription($"*{items}*").WithImageUrl(unboxed[0].Url);
        var components = new ComponentBuilder()
            .WithButton(emote: new Emoji(Emotes.Repeat), customId: "unbox-again", style: ButtonStyle.Secondary)
            .WithButton(emote: new Emoji(Emotes.Book), customId: "unbox-stats", style: ButtonStyle.Primary, disabled: opened == 1);
        if (opened == 69) components.WithButton(emote: new Emoji(Emotes.Money), url: "https://www.gamblersanonymous.org/ga/", style: ButtonStyle.Link);

        await SendOpeningAnimationAsync(interaction, author, boxData.Gif);

        await interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed.Build();
            msg.Components = components.Build();
        });
    }

    private async Task SaveUnboxedAsync(ulong userId, Box box, IReadOnlyList<ItemData> unboxed)
    {
        if (userId != config.GetValue<ulong>("ids:owner")) await unboxService.UpdateOrSaveBoxAsync(box);

        foreach (var item in unboxed)
        {
            unboxTracker.AddEntry(userId, box, item.Name);
        }
    }

    private async Task SendOpeningAnimationAsync(SocketInteraction interaction, EmbedAuthorBuilder author, string url)
    {
        var embed = embedHandler.GetEmbed(string.Empty)
            .WithAuthor(author)
            .WithImageUrl(url)
            .Build();

        await interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed;
            msg.Components = new ComponentBuilder().Build();
        });
        await Task.Delay(3000); // Give the gif time to play
    }

    private async Task<IReadOnlyList<ItemData>> OpenAsync(Box box)
    {
        var items = await GetItemDataAsync(box);
        var bonusBoxes = new List<Box>() { Box.Confection, Box.Lucky };
        var unboxed = new List<ItemData>();
        var prevOdds = 0.00;
        var roll = _random.NextDouble() * 100;

        foreach (var item in items)
        {
            if (prevOdds <= roll && roll < prevOdds + item.Chance)
            {
                unboxed.Add(item);

                if (bonusBoxes.Contains(box))
                {
                    var bonusItem = BonusRoll(box, items, item.Name, roll);
                    if (bonusItem != null) unboxed.Add(bonusItem);
                }

                return unboxed;
            }

            prevOdds += item.Chance;
        }

        return unboxed;
    }

    private static ItemData? BonusRoll(Box box, IReadOnlyList<ItemData> items, string unboxed, double roll)
    {
        switch (box)
        {
            case Box.Confection: return _random.NextDouble() * 100 <= 1 ? new ItemData("Sprinkle Aura", 0.00, string.Empty) : null;
            case Box.Lucky when roll <= 32:
                while (true)
                {
                    var bonusRoll = _random.NextDouble() * 32;
                    var prevOdds = 0.00;

                    foreach (var item in items)
                    {
                        if (prevOdds <= bonusRoll && bonusRoll < prevOdds + item.Chance)
                        {
                            if (item.Name == unboxed) break;
                            return item;
                        }

                        prevOdds += item.Chance;
                    }
                }
            default: return null;
        }
    }

    private async Task<IReadOnlyList<ItemData>> GetItemDataAsync(Box box) =>
        await jsonFileReader.ReadAsync<IReadOnlyList<ItemData>>(Path.Combine("Data", "Boxes", $"{box}.json"));
}
