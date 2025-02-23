using Discord;
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("UnitTests")]
namespace Kozma.net.Src.Commands.Information;

public partial class FindLogs(IMemoryCache cache,
    IEmbedHandler embedHandler,
    ITradeLogService tradeLogService,
    IFileReader jsonFileReader,
    IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly MemoryCacheEntryOptions _cacheOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) };

    [SlashCommand(CommandIds.FindLogs, "Search the tradelog database for any item.")]
    public async Task ExecuteAsync(
        [Summary(description: "Item the bot should look for."), Autocomplete(), MinLength(3), MaxLength(69)] string item,
        [Summary(description: "How far back the bot should search. Default: 6 months."), MinValue(1), MaxValue(120)] int months = 6,
        [Summary(description: "Check for color variants / item family tree. Default: yes.")] bool variants = true,
        [Summary(description: "Filter out high value uvs. Default: no.")] bool clean = false,
        [Summary(description: "Check the mixed-trades channel. Default: yes.")] bool mixed = true)
    {
        var altered = item.CleanUp();

        var embed = embedHandler.GetEmbed($"Searching for {Format.Underline(item)}, I will dm you what I can find.")
            .WithDescription($"{Format.Header("Info & tips when searching:", level: 3)}\n- {Format.Bold("Slime boxes:")}\nIdentifier followed by {Format.Italics("Slime Lockbox")}.\nExample: QQQ Slime Lockbox\n" +
                $"- {Format.Bold("UV's:")}\nUse asi / ctr + med / high / very high / max.\n" +
                "The bot automatically swaps asi & ctr so you don't have to search twice.\nExample: Brandish ctr very high asi high\n" +
                $"- {Format.Bold("Equipment:")}\nThe bot looks for the entire family tree of your item!\n" +
                $"So when you lookup {Format.Italics("Brandish")} it will also match on {Format.Italics("Combuster")} & {Format.Italics("Acheron")}.\n" +
                $"- {Format.Bold("Color Themes:")}\nCertain colors with (expected) similar value are grouped for more results.\nSome examples include {Format.Italics("Divine")} & {Format.Italics("Volcanic")}, tech colors, standard colors, etc.\n" +
                $"- {Format.Bold("Sprite pods:")}\nType out as seen in game.\nExample: Drakon Pod (Divine)");

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
        if (Context.User.Id != config.GetValue<ulong>("ids:owner")) await tradeLogService.UpdateOrSaveItemAsync(altered);

        var matches = await SearchLogsAsync(altered, months, checkVariants: variants, checkClean: clean, checkMixed: mixed);
        await SendMatchesAsync(Context.User, matches, altered, item, months, checkVariants: variants);
    }

    public async Task<IEnumerable<LogGroups>> SearchLogsAsync(string item, int months, bool checkVariants, bool checkClean, bool checkMixed)
    {
        var cacheKey = $"{item}_{months}_{checkVariants}_{checkClean}_{checkMixed}";

        if (!cache.TryGetValue(cacheKey, out IEnumerable<LogGroups>? matches) || matches is null)
        {
            var reverse = new List<string>();
            var ignore = new List<string>();
            var items = new List<string>() { AttachUvsToBack(item) };
            var stopHere = DateTime.Now.AddMonths(-months);
            var cleanFilter = new List<string>() { "CTR HIGH", "CTR VERY HIGH", "ASI HIGH", "ASI VERY HIGH", "NORMAL HIGH", "NORMAL MAX", "SHADOW HIGH", "SHADOW MAX", "FIRE HIGH", "FIRE MAX", "SHOCK HIGH", "SHOCK MAX" };

            if (checkVariants) await AddVariantsAsync(items);
            if (items[0].Contains("ctr", StringComparison.OrdinalIgnoreCase) && items[0].Contains("asi", StringComparison.OrdinalIgnoreCase)) items.ForEach(item => reverse.Add(SwapUvs(item)));
            if (checkClean) items.ForEach(item => cleanFilter.ForEach(uv => ignore.Add($"{item} {uv}")));
            if (items[0].Contains("blaster", StringComparison.OrdinalIgnoreCase) && !items[0].Contains("nog", StringComparison.OrdinalIgnoreCase)) ignore.Add("NOG BLASTER");
            if (!items[0].Contains("recipe", StringComparison.OrdinalIgnoreCase)) ignore.Add("RECIPE");

            var commonFeatured = await jsonFileReader.ReadAsync<IEnumerable<string>>(Path.Combine("Data", "FindLogs", "CommonFeatured.json"));
            var skipSpecial = commonFeatured.Any(item => items[0].Contains(item, StringComparison.OrdinalIgnoreCase));
            matches = await tradeLogService.GetLogsAsync([.. items, .. reverse], stopHere, checkMixed, skipSpecial, ignore);

            cache.Set(cacheKey, matches, _cacheOptions);
            SaveCacheKey(cacheKey);
        }

        return matches;
    }

    public async Task SendMatchesAsync(SocketUser user, IEnumerable<LogGroups> matches, string item, string original, int months, bool checkVariants)
    {
        var matchCount = 0;

        try
        {
            foreach (var channel in matches)
            {
                var count = channel.Messages.Count;
                var charCount = 0;
                var embeds = new List<Embed> { embedHandler.GetBasicEmbed($"I found {count:N0} post{(count != 1 ? "s" : string.Empty)} in {channel.Channel}:").WithColor(Colors.Crown).Build() };
                matchCount += count;

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

            await FinishInteractionAsync(user, item, original, matchCount, months, checkVariants);
        }
        catch
        {
            await SendErrorEmbedAsync();
        }
    }

    private async Task FinishInteractionAsync(SocketUser user, string item, string original, int matchCount, int months, bool checkVariants)
    {
        var embed = embedHandler.GetEmbed($"I found {matchCount} message{(matchCount != 1 ? "s" : string.Empty)} containing {Format.Underline(original)}")
            .WithColor(Colors.Crown)
            .WithDescription($"By default I only look at tradelogs from the past {Format.Bold("6 months")}!\n" +
                $"If you want me to look past that use the {Format.Code("months")} option.\n\n" +
                $"- Only want to see your item and no variants?\nSet {Format.Code("variants")} to {Format.Italics("False")}.\n" +
                $"- Want to filter out higher value UV's?\nSet {Format.Code("clean")} to {Format.Italics("True")}.\n" +
                $"- Not interested in item trades?\nSet {Format.Code("mixed")} to {Format.Italics("False")}.\n\n" +
                $"If you notice a problem please contact {MentionUtils.MentionUser(config.GetValue<ulong>("ids:owner"))}!\n" +
                $"Did you know we have our own {Format.Url(Format.Bold("Discord server"), config.GetValue<string>("serverInvite"))}?");

        var spreadsheet = await jsonFileReader.ReadAsync<IEnumerable<string>>(Path.Combine("Data", "FindLogs", "Spreadsheet.json"));
        if (spreadsheet.Any(equipment => item.Contains(equipment, StringComparison.OrdinalIgnoreCase)))
        {
            embed.AddField(Emotes.Empty, $"{Format.Underline(original)} can be found on the {Format.Url(Format.Bold("merchant sheet"), "https://docs.google.com/spreadsheets/d/1h-SoyMn3kVla27PRW_kQQO6WefXPmLZYy7lPGNUNW7M/htmlview#")}.");
        }

        var components = new ComponentBuilder().WithButton(label: "Delete messages", customId: ComponentIds.ClearMessages, style: ButtonStyle.Primary);
        if (months < 24) components.WithButton(label: "Search all tradelogs", customId: $"{ComponentIds.FindLogsBase}{(checkVariants ? ComponentIds.FindLogsVar : ComponentIds.FindLogsSingle)}", style: ButtonStyle.Primary);

        cache.Set($"{CommandIds.FindLogs}_{user.Id}", matchCount, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) });

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
                $"{Format.Italics("Allow direct messages from server members")} in User Settings > Privacy & Safety\n\nAnd don't block me!")
                .WithColor(Colors.Error);

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
    }

    internal static string AttachUvsToBack(string item)
    {
        var uvTypes = new List<string>() { "CTR", "ASI", "NORMAL", "SHADOW", "FIRE", "SHOCK", "POISON", "STUN", "FREEZE", "ELEMENTAL", "PIERCING" };
        var uvGrades = new List<string>() { "LOW", "MED", "HIGH", "VERY", "MAX" };
        var input = item.Split(" ");

        for (int i = 0; i < input.Length; i++)
        {
            foreach (var type in uvTypes)
            {
                if (!string.Equals(input[i], type, StringComparison.OrdinalIgnoreCase)) continue;

                foreach (var grade in uvGrades)
                {
                    if (i + 1 >= input.Length || !string.Equals(input[i + 1], grade, StringComparison.OrdinalIgnoreCase)) continue;

                    var uv = grade == "VERY" && (i + 2 < input.Length && string.Equals(input[i + 2], "HIGH", StringComparison.OrdinalIgnoreCase)) ? type + " VERY HIGH" : type + " " + grade;
                    item = (item.Replace(uv, string.Empty, StringComparison.OrdinalIgnoreCase) + " " + uv).RemoveExtraWhiteSpace().Trim();
                }
            }
        }

        return item;
    }

    internal async Task AddVariantsAsync(List<string> items)
    {
        var item = items[0];
        var exceptions = new List<string> { "drakon", "maskeraith", "nog" };
        if (exceptions.Any(keyword => item.Contains(keyword, StringComparison.OrdinalIgnoreCase))) return;

        var equipmentFamilies = await jsonFileReader.ReadAsync<IReadOnlyDictionary<string, List<string>>>(Path.Combine("Data", "FindLogs", "EquipmentFamilies.json"));
        var family = equipmentFamilies.FirstOrDefault(f => f.Value.Any(name => item.Contains(name, StringComparison.OrdinalIgnoreCase)));

        if (!family.Equals(default(KeyValuePair<string, List<string>>)))
        {
            var match = family.Value.First(name => item.Contains(name, StringComparison.OrdinalIgnoreCase));
            if (match == "AVENGER" && items[0].Contains("helm", StringComparison.OrdinalIgnoreCase)) return;

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
                if (set.Key == "GEMS" && GemExceptionRegex().IsMatch(item)) break;
                if (set.Key == "SNIPES" && (item.Contains("slime", StringComparison.OrdinalIgnoreCase) ||
                    item.Contains("plume", StringComparison.OrdinalIgnoreCase) || item.Contains("pepper", StringComparison.OrdinalIgnoreCase))) break;

                var template = item.Replace(color, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
                if (color == "ROSE" && ((template.Contains("tabard", StringComparison.OrdinalIgnoreCase) || template.Contains("chapeau", StringComparison.OrdinalIgnoreCase)) || RoseColorRegex().IsMatch(template))) break;
                if (set.Key == "GEMS" && template.Contains("floating", StringComparison.OrdinalIgnoreCase)) template = template.Replace(" s", string.Empty, StringComparison.OrdinalIgnoreCase);

                items.Clear();
                if (set.Key == "OBSIDIAN" || set.Key == "GEMS" || set.Key.Contains("ROSE", StringComparison.OrdinalIgnoreCase))
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

    internal static string SwapUvs(string name)
    {
        var nameList = name.Split(' ');
        var ctrIndex = Array.IndexOf(nameList, "CTR");
        var asiIndex = Array.IndexOf(nameList, "ASI");

        if (ctrIndex == -1 || asiIndex == -1) return name;

        var separator = " ";
        var minIndex = Math.Min(ctrIndex, asiIndex);
        var maxIndex = Math.Max(ctrIndex, asiIndex);
        var swapped = new StringBuilder();

        swapped.Append(string.Join(separator, nameList.Take(minIndex)) + separator);
        swapped.Append(string.Join(separator, nameList.Skip(maxIndex)) + separator);
        swapped.Append(string.Join(separator, nameList.Skip(minIndex).Take(maxIndex - minIndex)) + separator);

        return swapped.ToString().Trim();
    }

    // Store the keys from searches so they can be deleted when logs get updated
    private void SaveCacheKey(string key)
    {
        cache.TryGetValue(CommandIds.FindLogs, out List<string>? keys);
        keys ??= [];

        keys.Add(key);
        cache.Set(CommandIds.FindLogs, keys, _cacheOptions);
    }

    [GeneratedRegex("(bout|rose|tabard|chapeau|buckled|clover|pipe|lumberfell)")]
    private static partial Regex GemExceptionRegex();

    [GeneratedRegex(@"\b(black|red|white|blue|gold|green|coral|violet|moonstone|malachite|garnet|amethyst|citrine|prismatic|aquamarine|turquoise)\b")]
    private static partial Regex RoseColorRegex();
}
