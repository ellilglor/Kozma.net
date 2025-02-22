using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Logging;
using Kozma.net.Src.Models;
using Kozma.net.Src.Trackers;

namespace Kozma.net.Src.Helpers;
public record PunchReward(string Author, string Url);

public class PunchHelper(IPunchTracker punchTracker, IFileReader jsonFileReader, IBotLogger logger) : IPunchHelper
{
    private static readonly Random _random = new();

    public EmbedAuthorBuilder GetAuthor() =>
        new EmbedAuthorBuilder().WithName("Punch").WithIconUrl("https://media3.spiralknights.com/wiki-images/archive/1/1b/20200502113903!Punch-Mugshot.png");

    public async Task SendWaitingAnimationAsync(EmbedBuilder embed, SocketInteraction interaction, string url, int delayInMs = 1500)
    {
        await interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed.WithAuthor(GetAuthor()).WithImageUrl(url).Build();
            msg.Components = new ComponentBuilder().Build();
        });
        await Task.Delay(delayInMs); // Give the gif time to play
    }

    public MessageComponent GetComponents(int uvCount, int lockCount = 0)
    {
        return new ComponentBuilder()
            .WithButton(emote: new Emoji(Emotes.Locked), customId: ComponentIds.PunchInfoBase + ComponentIds.PunchInfoLock, style: ButtonStyle.Primary)
            .WithButton(emote: new Emoji(Emotes.One), customId: $"{ComponentIds.PunchLock}1", style: ButtonStyle.Secondary, disabled: uvCount < 1)
            .WithButton(emote: new Emoji(Emotes.Two), customId: $"{ComponentIds.PunchLock}2", style: ButtonStyle.Secondary, disabled: uvCount < 2)
            .WithButton(emote: new Emoji(Emotes.Three), customId: $"{ComponentIds.PunchLock}3", style: ButtonStyle.Secondary, disabled: uvCount < 3)
            .WithButton(emote: new Emoji(Emotes.Book), customId: ComponentIds.PunchInfoBase + ComponentIds.PunchInfoStats, style: ButtonStyle.Primary)
            .WithButton(emote: new Emoji(Emotes.Dice), customId: ComponentIds.PunchInfoBase + ComponentIds.PunchInfoGamble, style: ButtonStyle.Primary, row: 2)
            .WithButton(emote: new Emoji(Emotes.One), customId: $"{ComponentIds.PunchGamble}1", style: ButtonStyle.Secondary, disabled: lockCount > 0)
            .WithButton(emote: new Emoji(Emotes.Two), customId: $"{ComponentIds.PunchGamble}2", style: ButtonStyle.Secondary, disabled: lockCount > 1)
            .WithButton(emote: new Emoji(Emotes.Three), customId: $"{ComponentIds.PunchGamble}3", style: ButtonStyle.Secondary, disabled: lockCount > 2)
            .WithButton(emote: new Emoji(Emotes.QMark), customId: ComponentIds.PunchInfoBase + ComponentIds.PunchInfoOdds, style: ButtonStyle.Primary)
            .Build();
    }

    public string RollUv(ulong id, PunchItem item, IReadOnlyCollection<string> uvs, bool crafting = false)
    {
        var uvGrade = GetUvGrade(item.Type);
        var uvType = GetUvType(item.Type, crafting);

        while (uvs.Any(uv => uv.Contains(uvType, StringComparison.OrdinalIgnoreCase)))
        {
            uvType = GetUvType(item.Type, crafting);
        }

        punchTracker.AddEntry(id, item.Name, uvType, uvGrade);

        return $"{uvType}:\n{uvGrade}";
    }

    private static string GetUvGrade(ItemType type)
    {
        var gradeRoll = _random.Next(1, 10001);

        var grades = new[]
        {
            (245, type == ItemType.Weapon || type == ItemType.Bomb ? "Very High" : "Maximum"), // 2.45% chance
            (732, "High"), // 4.87% chance
            (2683, "Medium"), // 19.51% chance
            (10000, "Low")  // 73.17% chance
        };

        return grades.FirstOrDefault(g => gradeRoll <= g.Item1).Item2;
    }

    private static string GetUvType(ItemType type, bool crafting)
    {
        if (type == ItemType.Weapon || type == ItemType.Bomb)
        {
            var limit = type == ItemType.Weapon ? 8 : 7;
            var typeRoll = _random.Next(0, limit);

            return typeRoll switch
            {
                0 => "Damage Bonus vs Undead",
                1 => "Damage Bonus vs Slime",
                2 => "Damage Bonus vs Construct",
                3 => "Damage Bonus vs Gremlin",
                4 => "Damage Bonus vs Fiend",
                5 => "Damage Bonus vs Beast",
                6 => "Charge Time Reduction",
                _ => "Attack Speed Increase"
            };
        }
        else
        {
            var limit = type == ItemType.Armor || type == ItemType.Shield && crafting ? 11 : 4;
            var typeRoll = _random.Next(0, limit);

            return typeRoll switch
            {
                0 => "Increased Normal Defense",
                1 => "Increased Piercing Defense",
                2 => "Increased Elemental Defense",
                3 => "Increased Shadow Defense",
                4 => "Increased Stun Resistance",
                5 => "Increased Freeze Resistance",
                6 => "Increased Poison Resistance",
                7 => "Increased Fire Resistance",
                8 => "Increased Shock Resistance",
                9 => "Increased Curse Resistance",
                _ => "Increased Sleep Resistance"
            };
        }
    }

    public async Task<(string desc, string image)> CheckForGmAsync(string user, ItemType type, IReadOnlyCollection<string> uvs)
    {
        var won = type switch
        {
            ItemType.Weapon => HasRequiredUVs(uvs, 2, "Very High", "Charge", "Attack"),
            ItemType.Armor => HasRequiredUVs(uvs, 3, "Max", "Shadow", "Normal", "Fire"),
            _ => false
        };

        if (!won) return (string.Empty, string.Empty);

        logger.Log(LogLevel.Special, $"{user} rolled a GM item");

        var rewards = await jsonFileReader.ReadAsync<IReadOnlyList<PunchReward>>(Path.Combine("Data", "Punch.json"));
        var reward = rewards[_random.Next(rewards.Count)];
        return ($"Congratulations! You created a GM item.\nAs a reward you get a random Spiral Knights meme.\nAuthor: {Format.Bold(reward.Author)}", reward.Url);
    }

    static bool HasRequiredUVs(IReadOnlyCollection<string> uvs, int requiredCount, string mustContain, params string[] options) =>
        uvs.Count(uv => uv.Contains(mustContain, StringComparison.OrdinalIgnoreCase) && options.Any(option => uv.Contains(option, StringComparison.OrdinalIgnoreCase))) >= requiredCount;
}
