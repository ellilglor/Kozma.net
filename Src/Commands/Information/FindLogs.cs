﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Models.Entities;
using Kozma.net.Src.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace Kozma.net.Src.Commands.Information;

public partial class FindLogs(IMemoryCache cache,
    IEmbedHandler embedHandler,
    ITradeLogService tradeLogService,
    IFileReader jsonFileReader,
    IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    // TODO? change choice options to bool
    [SlashCommand("findlogs", "Search the tradelog database for any item.")]
    public async Task ExecuteAsync(
        [Summary(description: "Item the bot should look for."), MinLength(3), MaxLength(69)] string item,
        [Summary(description: "How far back the bot should search. Default: 6 months."), MinValue(1), MaxValue(120)] int months = 6,
        [Summary(description: "Check for color variants / item family tree. Default: yes."), Choice("Yes", "variant-search"), Choice("No", "single-search")] string? variants = null,
        [Summary(description: "Filter out high value uvs. Default: no."), Choice("Yes", "clean-search"), Choice("No", "dirty-search")] string? clean = null,
        [Summary(description: "Check the mixed-trades channel. Default: yes."), Choice("Yes", "mixed-search"), Choice("No", "mixed-ignore")] string? mixed = null)
    {
        var checkVariants = string.IsNullOrEmpty(variants) || variants == "variant-search";
        var checkClean = !string.IsNullOrEmpty(clean) && clean == "clean-search";
        var checkMixed = string.IsNullOrEmpty(mixed) || mixed == "mixed-search";
        var altered = item.CleanUp();

        var embed = embedHandler.GetEmbed($"Searching for __{item}__, I will dm you what I can find.")
            .WithDescription("### Info & tips when searching:\n- **Slime boxes**:\ncombination followed by *slime lockbox*\nExample: QQQ Slime Lockbox\n" +
                "- **UV's**:\nuse asi / ctr + med / high / very high / max\n" +
                "The bot automatically swaps asi & ctr so you don't have to search twice.\nExample: Brandish ctr very high asi high\n" +
                "- **Equipment**:\nThe bot looks for the entire family tree of your item!\n" +
                "So when you lookup *brandish* it will also match on *Combuster* & *Acheron*\n" +
                "- **Color Themes**:\ncertain colors with (expected) similar value are grouped for more results." +
                " Some examples include *Divine* & *Volcanic*, tech colors, standard colors, etc.\n" +
                "- **Sprite pods**:\ntype out as seen in game\nExample: Drakon Pod (Divine)");

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
        if (Context.User.Id != config.GetValue<ulong>("ids:owner")) await tradeLogService.UpdateOrSaveItemAsync(altered);
        await SearchLogsAsync(altered, item, months, checkVariants, checkClean, checkMixed);
    }

    public async Task SearchLogsAsync(string item, string original, int months, bool checkVariants, bool checkClean, bool checkMixed, SocketUser? user = null)
    {
        var cacheKey = $"{item}_{original}_{months}_{checkVariants}_{checkClean}_{checkMixed}";
        var cmdUser = user ?? Context.User;

        if (!cache.TryGetValue(cacheKey, out IEnumerable<LogGroups>? matches) || matches is null)
        {
            var reverse = new List<string>();
            var ignore = new List<string>();
            var items = new List<string>() { item };
            var stopHere = DateTime.Now.AddMonths(-months);
            var cleanFilter = new List<string>() { "ctr high", "ctr very high", "asi high", "asi very high", "normal high", "normal max", "shadow high", "shadow max", "fire high", "fire max", "shock high", "shock max" };

            AttachUvsToBack(items);
            if (checkVariants) await AddVariantsAsync(items);
            if (items[0].Contains("ctr", StringComparison.OrdinalIgnoreCase) && items[0].Contains("asi", StringComparison.OrdinalIgnoreCase)) items.ForEach(item => reverse.Add(SwapUvs(item)));
            if (checkClean) items.ForEach(item => cleanFilter.ForEach(uv => ignore.Add($"{item} {uv}")));
            if (items[0].Contains("blaster", StringComparison.OrdinalIgnoreCase) && !items[0].Contains("nog", StringComparison.OrdinalIgnoreCase)) ignore.Add("nog blaster");
            if (!items[0].Contains("recipe", StringComparison.OrdinalIgnoreCase)) ignore.Add("recipe");

            var commonFeatured = await jsonFileReader.ReadAsync<IEnumerable<string>>(Path.Combine("Data", "FindLogs", "CommonFeatured.json"));
            var skipSpecial = commonFeatured.Any(item => items[0].Contains(item, StringComparison.OrdinalIgnoreCase));
            matches = await tradeLogService.GetLogsAsync([.. items, .. reverse], stopHere, checkMixed, skipSpecial, ignore);

            cache.Set(cacheKey, matches, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) });
        }

        var matchCount = matches.Sum(collection => collection.Messages.Count);
        var sentMatchesSuccesfully = await SendMatchesAsync(matches, cmdUser);
        if (sentMatchesSuccesfully) await FinishInteractionAsync(item, original, matchCount, months, checkVariants, cmdUser);
    }

    private async Task<bool> SendMatchesAsync(IEnumerable<LogGroups> matches, SocketUser user)
    {
        try
        {
            foreach (var channel in matches)
            {
                var count = channel.Messages.Count;
                var charCount = 0;
                var embeds = new List<Embed>()
                {
                    embedHandler.GetBasicEmbed($"I found {count:N0} post{(count != 1 ? "s" : string.Empty)} in {channel.Channel}:").WithColor(Colors.Crown).Build()
                };

                foreach (var message in channel.Messages)
                {
                    if ((charCount + message.OriginalContent.Length > ExtendedDiscordConfig.MaxCharsAcrossEmbeds) || embeds.Count == DiscordConfig.MaxEmbedsPerMessage)
                    {
                        await user.SendMessageAsync(embeds: [.. embeds]);
                        embeds.Clear();
                        charCount = 0;
                    }

                    charCount += message.OriginalContent.Length;

                    embeds.Add(embedHandler.GetBasicEmbed(message.Date.ToString("ddd, dd MMM yyyy"))
                        .WithUrl(message.MessageUrl)
                        .WithImageUrl(message.Image)
                        .WithDescription(message.OriginalContent.Length > ExtendedDiscordConfig.MaxEmbedDescChars ? message.OriginalContent.Substring(0, ExtendedDiscordConfig.MaxEmbedDescChars) : message.OriginalContent)
                        .Build());
                }

                if (embeds.Count > 0) await user.SendMessageAsync(embeds: [.. embeds]);
            }

            return true;
        }
        catch
        {
            await SendErrorEmbedAsync();
            return false;
        }
    }

    private async Task FinishInteractionAsync(string item, string copy, int matchCount, int months, bool checkVariants, SocketUser user)
    {
        var embed = embedHandler.GetEmbed($"I found {matchCount} message{(matchCount != 1 ? "s" : string.Empty)} containing __{copy}__")
            .WithColor(Colors.Crown)
            .WithDescription("By default I only look at tradelogs from the past **6 months**!\n" +
                "If you want me to look past that use the `months` option.\n\n" +
                "- Only want to see your item and no variants?\nSet `variants` to *NO*.\n" +
                "- Want to filter out higher value UV's?\nSet `clean` to *YES*.\n" +
                "- Not interested in item trades?\nSet `mixed` to *NO*.\n\n" +
                $"If you notice a problem please contact <@{config.GetValue<ulong>("ids:owner")}>!\n" +
                $"Did you know we have our own [**Discord server**]({config.GetValue<string>("serverInvite")} 'Kozma's Backpack Discord server')?");

        var spreadsheet = await jsonFileReader.ReadAsync<IEnumerable<string>>(Path.Combine("Data", "FindLogs", "Spreadsheet.json"));
        if (spreadsheet.Any(equipment => item.Contains(equipment, StringComparison.OrdinalIgnoreCase)))
        {
            embed.AddField("** **", $"__{copy}__ can be found on the [**merchant sheet**](https://docs.google.com/spreadsheets/d/1h-SoyMn3kVla27PRW_kQQO6WefXPmLZYy7lPGNUNW7M/htmlview#).");
        }

        var components = new ComponentBuilder().WithButton(label: "Delete messages", customId: "clear-messages", style: ButtonStyle.Primary);
        if (months < 24) components.WithButton(label: "Search all tradelogs", customId: $"research-{(checkVariants ? "var" : "single")}", style: ButtonStyle.Primary);

        try
        {
            await user.SendMessageAsync(embed: embed.Build(), components: components.Build());
        }
        catch
        {
            await SendErrorEmbedAsync();
        }
    }

    // The server responded with error 50007: Cannot send messages to this user
    private async Task SendErrorEmbedAsync()
    {
        var embed = embedHandler.GetEmbed("I can't send you any messages!")
                .WithDescription("Make sure you have the following enabled:\n" +
                "*Allow direct messages from server members* in User Settings > Privacy & Safety\n\nAnd don't block me!")
                .WithColor(Colors.Error);

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
    }

    private static void AttachUvsToBack(List<string> items)
    {
        var uvTypes = new List<string>() { "ctr", "asi", "normal", "shadow", "fire", "shock", "poison", "stun", "freeze", "elemental", "piercing" };
        var uvGrades = new List<string>() { "low", "med", "high", "very", "max" };
        var input = items[0].Split(" ");

        for (int i = 0; i < input.Length; i++)
        {
            foreach (var type in uvTypes)
            {
                if (!string.Equals(input[i], type, StringComparison.OrdinalIgnoreCase)) continue;

                foreach (var grade in uvGrades)
                {
                    if (i + 1 >= input.Length || !string.Equals(input[i + 1], grade, StringComparison.OrdinalIgnoreCase)) continue;

                    var uv = grade == "very" && (i + 2 < input.Length && string.Equals(input[i + 2], "high", StringComparison.OrdinalIgnoreCase)) ? type + " very high" : type + " " + grade;
                    items[0] = (items[0].Replace(uv, string.Empty, StringComparison.OrdinalIgnoreCase) + " " + uv).Replace("  ", " ", StringComparison.OrdinalIgnoreCase).Trim();
                }
            }
        }
    }

    private async Task AddVariantsAsync(List<string> items)
    {
        var item = items[0];
        var exceptions = new List<string> { "drakon", "maskeraith", "nog" };
        if (exceptions.Any(keyword => item.Contains(keyword, StringComparison.OrdinalIgnoreCase))) return;

        var equipmentFamilies = await jsonFileReader.ReadAsync<IReadOnlyDictionary<string, List<string>>>(Path.Combine("Data", "FindLogs", "EquipmentFamilies.json"));
        var family = equipmentFamilies.FirstOrDefault(f => f.Value.Any(name => item.Contains(name, StringComparison.OrdinalIgnoreCase)));

        if (!family.Equals(default(KeyValuePair<string, List<string>>)))
        {
            var match = family.Value.First(name => item.Contains(name, StringComparison.OrdinalIgnoreCase));
            var uvs = item.Replace(match, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

            items.Clear();
            family.Value.ForEach(entry => items.Add($"{entry} {uvs}".Trim()));

            return;
        }

        var colorSets = await jsonFileReader.ReadAsync<IReadOnlyDictionary<string, List<string>>>(Path.Combine("Data", "FindLogs", "Colors.json"));
        foreach (var set in colorSets)
        {
            foreach (var color in set.Value)
            {
                if (!item.Contains(color, StringComparison.OrdinalIgnoreCase)) continue;
                if (set.Key == "gems" && GemExceptionRegex().IsMatch(item)) break;
                if (set.Key == "snipes" && (item.Contains("slime", StringComparison.OrdinalIgnoreCase) ||
                    item.Contains("plume", StringComparison.OrdinalIgnoreCase) || item.Contains("pepper", StringComparison.OrdinalIgnoreCase))) break;

                var template = item.Replace(color, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
                if (color == "rose" && ((template.Contains("tabard", StringComparison.OrdinalIgnoreCase) || template.Contains("chapeau", StringComparison.OrdinalIgnoreCase)) || RoseColorRegex().IsMatch(template))) break;

                items.Clear();
                if (set.Key == "obsidian" || set.Key.Contains("rose", StringComparison.OrdinalIgnoreCase))
                {
                    set.Value.ForEach(value => items.Add($"{template} {value}".Trim()));
                }
                else
                {
                    set.Value.ForEach(value => items.Add($"{value} {template}".Trim()));
                }

                return;
            }
        }
    }

    private static string SwapUvs(string name)
    {
        var nameList = name.Split(' ');
        int ctr = Array.IndexOf(nameList, "ctr");
        int asi = Array.IndexOf(nameList, "asi");
        int minIndex = Math.Min(ctr, asi);
        int maxIndex = Math.Max(ctr, asi);
        var swapped = new StringBuilder();

        swapped.Append(string.Join(" ", nameList.Take(minIndex)) + " ");
        swapped.Append(string.Join(" ", nameList.Skip(maxIndex)) + " ");
        swapped.Append(string.Join(" ", nameList.Skip(minIndex).Take(maxIndex - minIndex)) + " ");

        return swapped.ToString().Trim();
    }

    [GeneratedRegex("(bout|rose|tabard|chapeau|buckled|clover|pipe|lumberfell)")]
    private static partial Regex GemExceptionRegex();

    [GeneratedRegex(@"\b(black|red|white|blue|gold|green|coral|violet|moonstone|malachite|garnet|amethyst|citrine|prismatic|aquamarine|turquoise)\b")]
    private static partial Regex RoseColorRegex();
}
