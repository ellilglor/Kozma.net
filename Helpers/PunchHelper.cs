﻿using Discord;
using Discord.Interactions;
using Kozma.net.Enums;
using Kozma.net.Models;

namespace Kozma.net.Helpers;

public class PunchHelper : IPunchHelper
{
    private static readonly Random _random = new();

    public EmbedAuthorBuilder GetAuthor()
    {
        return new EmbedAuthorBuilder().WithName("Punch").WithIconUrl("https://media3.spiralknights.com/wiki-images/archive/1/1b/20200502113903!Punch-Mugshot.png");
    }

    public PunchItem? GetItem(PunchOption item)
    {
        return item switch
        {
            PunchOption.Brandish => new PunchItem("Brandish", ItemType.Weapon,
                "https://media3.spiralknights.com/wiki-images/2/22/Brandish-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643184252133406/sword.gif"),
            PunchOption.Mixmaster => new PunchItem("Overcharged Mixmaster", ItemType.Weapon,
                "https://media3.spiralknights.com/wiki-images/f/fd/Overcharged_Mixmaster-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643185170686064/mixmaster.gif"),
            PunchOption.Bomb => new PunchItem("Blast Bomb", ItemType.Bomb,
                "https://media3.spiralknights.com/wiki-images/c/c2/Blast_Bomb-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643183866253392/bomb.gif"),
            PunchOption.Shield => new PunchItem("Swiftstrike Buckler", ItemType.Shield,
                "https://media3.spiralknights.com/wiki-images/5/5b/Swiftstrike_Buckler-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643184688337027/shield.gif"),
            PunchOption.Helmet => new PunchItem("Black Kat Cowl", ItemType.Armor,
                "https://media3.spiralknights.com/wiki-images/2/20/Black_Kat_Cowl-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643185539776532/helm.gif"),
            _ => null
        };
    }

    public PunchOption? ConvertToPunchOption(string item)
    {
        return item switch
        {
            "Brandish" => PunchOption.Brandish,
            "Overcharged Mixmaster" => PunchOption.Mixmaster,
            "Blast Bomb" => PunchOption.Bomb,
            "Swiftstrike Buckler" => PunchOption.Shield,
            "Black Kat Cowl" => PunchOption.Helmet,
            _ => null
        };
    }

    public async Task SendWaitingAnimationAsync(EmbedBuilder embed, SocketInteractionContext context, string url, int delay)
    {
        await context.Interaction.ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed.WithAuthor(GetAuthor()).WithImageUrl(url).Build(); ;
            msg.Components = new ComponentBuilder().Build();
        });
        await Task.Delay(delay); // Give the gif time to play
    }

    public MessageComponent GetComponents(bool lock1, bool lock2, bool lock3, bool gamble1, bool gamble2, bool gamble3)
    {
        return new ComponentBuilder()
            .WithButton(emote: new Emoji("\U0001F512"), customId: "punch-info-lock", style: ButtonStyle.Primary)
            .WithButton(emote: new Emoji("1️⃣"), customId: "punch-lock-1", style: ButtonStyle.Secondary, disabled: lock1)
            .WithButton(emote: new Emoji("2️⃣"), customId: "punch-lock-2", style: ButtonStyle.Secondary, disabled: lock2)
            .WithButton(emote: new Emoji("3️⃣"), customId: "punch-lock-3", style: ButtonStyle.Secondary, disabled: lock3)
            .WithButton(emote: new Emoji("\U0001F4D8"), customId: "punch-info-stats", style: ButtonStyle.Primary)
            .WithButton(emote: new Emoji("\U0001F3B2"), customId: "punch-info-gamble", style: ButtonStyle.Primary, row: 2)
            .WithButton(emote: new Emoji("1️⃣"), customId: "punch-gamble-1", style: ButtonStyle.Secondary, disabled: gamble1)
            .WithButton(emote: new Emoji("2️⃣"), customId: "punch-gamble-2", style: ButtonStyle.Secondary, disabled: gamble2)
            .WithButton(emote: new Emoji("3️⃣"), customId: "punch-gamble-3", style: ButtonStyle.Secondary, disabled: gamble3)
            .WithButton(emote: new Emoji("❔"), customId: "punch-info-odds", style: ButtonStyle.Primary)
            .Build();
    }

    public string RollUv(ItemType type, List<string> uvs, bool crafting = false)
    {
        var uvGrade = GetUvGrade(type);
        var uvType = GetUvType(type, crafting);

        while (uvs.Any(uv => uv.Contains(uvType)))
        {
            uvType = GetUvType(type, crafting);
        }

        return $"{uvType}:\n{uvGrade}";
    }

    private static string GetUvGrade(ItemType type)
    {
        var gradeRoll = _random.Next(1, 10001);

        if (gradeRoll <= 245) // 2.45% chance
        {
            return type == ItemType.Weapon || type == ItemType.Bomb ? "Very High" : "Maximum";
        }
        else if (gradeRoll <= 732) // 4.87% chance
        {
            return "High";
        }
        else if (gradeRoll <= 2683) // 19.51% chance
        {
            return "Medium";
        }
        else // 73.17% chance
        {
            return "Low";
        }
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
            var limit = type == ItemType.Armor || (type == ItemType.Shield && crafting) ? 11 : 4;
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
}
