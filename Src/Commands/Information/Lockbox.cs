using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Handlers;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kozma.net.Src.Commands.Information;

public partial class Lockbox(IEmbedHandler embedHandler) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Dictionary<LockboxOption, string> _lockboxes = BuildLockboxes();

    [SlashCommand(CommandIds.LockBox, "Get the drops from a (slime) lockbox or find what box drops your item.")]
    public async Task ExecuteAsync(
        [Summary(name: "boxes", description: "Get the odds from a lockbox.")] LockboxOption? box = null,
        [Summary(name: "slime", description: "Find where you can find a special themed box."), MinLength(3), MaxLength(69)] string? slimeCode = null,
        [Summary(description: "Find which lockbox drops your item."), MinLength(3), MaxLength(69)] string? item = null)
    {
        var embed = embedHandler.GetEmbed("Please select 1 of the given options.");
        var optionCount = (box.HasValue ? 1 : 0) + (!string.IsNullOrEmpty(slimeCode) ? 1 : 0) + (!string.IsNullOrEmpty(item) ? 1 : 0);

        // Only 1 option should be selected
        if (optionCount != 1)
        {
            await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
            return;
        }

        if (box.HasValue && _lockboxes.TryGetValue(box.Value, out var match))
        {
            var selectedBox = (LockboxOption)box;
            var newTitle = selectedBox.ToString();

            if (box != LockboxOption.Colors)
            {
                embed.WithThumbnailUrl(selectedBox.ConvertToBox().ToBoxData().Url);
                newTitle = string.Concat(newTitle, " Lockbox");
            }

            embed.WithTitle($"{newTitle.ToUpper(CultureInfo.InvariantCulture)}:")
                .WithDescription(match);
        }

        if (!string.IsNullOrEmpty(slimeCode))
        {
            embed.WithTitle(GetSlimeboxDescription(slimeCode.ToUpper(CultureInfo.InvariantCulture)));
        }

        if (!string.IsNullOrEmpty(item))
        {
            var desc = FindItem(item);

            embed.WithTitle(string.IsNullOrEmpty(desc) ? $"I didn't find a box containing __{item}__." : $"These lockboxes contain __{item}__:")
                .WithDescription(desc);
        }

        var components = new ComponentBuilder()
            .WithButton(label: "Lockboxes", url: "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview", style: ButtonStyle.Link)
            .WithButton(label: "Slime Lockboxes", url: "https://docs.google.com/spreadsheets/d/1f9KQlDcQcoK3K2z6hc7ZTWD_SnrikdTkTXGppneq0YU/htmlview", style: ButtonStyle.Link);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed.Build();
            msg.Components = components.Build();
        });
    }

    private static Dictionary<LockboxOption, string> BuildLockboxes()
    {
        return new Dictionary<LockboxOption, string>
        {
            { LockboxOption.Colors, "- **97.85%** for Cool, Regal, Military, Heavy, Fancy, Dusky or Toasty.\n- **1.96%** for Divine or Volcanic\n- **0.19%** for Prismatic." },
            { LockboxOption.Copper, "- **1.92%** for a Shadow Key.\n- **76.78%** for Binoculars, Flower, Headband or Plume.\n- **19.19%** for Long Feather or Pipe." +
                "\n- **3.84%** for Wolver tail or Prismatic glow-eyes.\n- **0.19%** for Twinkle Aura or Twilight Aura." },
            { LockboxOption.Steel, "- **1.92%** for a Shadow Key.\n- **76.78%** for Bolted Vee, Wide Vee, Mecha Wings or Game Face.\n- **19.19%** for Vertical Vents or Spike Mohawk." +
                "\n- **3.84%** for Ankle Booster or Aero Fin.\n- **0.19%** for Shoulder Booster or Flame Aura." },
            { LockboxOption.Silver, "- **1.92%** for a Shadow Key.\n- **76.78%** for Vitakit, Targeting Module, Binocular Visor or Helm-Mounted Display.\n- **19.19%** for Maedate or Intel Tube." +
                "\n- **3.84%** for Giga Shades.\n- **0.19%** for Wings (50%) or Divine/Volcanic/Prismatic Halo (50%)." },
            { LockboxOption.Platinum, "- **1.92%** for a Shadow Key.\n- **76.78%** for Com Unit, Knight Vision Goggles or Goggles.\n- **19.19%** for Sensor Unit or Bomb Bandolier." +
                "\n- **3.84%** for Mohawk, Devious Horns or Scarf.\n- **0.19%** for Unclean Aura or Ghostly Aura." },
            { LockboxOption.Gold, "- **1.92%** for a Shadow Key.\n- **76.78%** for Canteen, Ribbon or Maid Headband.\n- **19.19%** for Monocle or Glasses.\n- **3.84%** for Mustache or Round Shades.\n- **0.19%** for Dapper Combo or Toupee." },
            { LockboxOption.Titanium, "- **1.92%** for a Shadow Key.\n- **76.78%** for Helm Guards, Munitions Pack, Barrel Belly or Vented Visor.\n- **19.19%** for Headlamp or Side Blade." +
                "\n- **3.84%** for Rebreather or Parrying Blade.\n- **0.19%** for Vial Bandolier." },
            { LockboxOption.Iron, "- **1.92%** for a Shadow Key.\n- **20% for one of the following:**\n - **76.78%** for Vitakit, Canteen or Barrel Belly.\n - **19.19%** for Side Blade or Bomb Bandolier.\n - **3.84%** for Wolver Tail or Parrying Blade." +
                "\n - **0.19%** with **50%** for Wings and **50%** to get an Aura: **27.78%** for Twinkle, Ghostly and Unclean. **13.89%** for Twilight & **2.77%** for Flame.\n- **80% for one of the following:**" +
                "\n - **76.78%** for Plume, Ribbon, Vented Visor, Binocular Visor, Knight Vision Goggles, Helm-Mounted Display, Goggles, Com Unit, Mecha Wings, Helm Guards, Bolted Vee, Headband, Wide Vee, Maid Headband or Flower." +
                "\n - **19.19%** for Long Feather, Vertical Vents, Pipe, Glasses or Maedate.\n - **3.84%** with **24.39%** for Scarf, Mustache and Mohawk and **2.44%** for Prismatic Glow-Eyes." +
                "\n - **0.19%** with **43.48%** for Dapper Combo and Toupee & **13.04%** for Divine/Volcanic/Prismatic Halo." },
            { LockboxOption.Mirrored, "- **90.91%** for the following eyes:\n - Cheeky, Closed, Dot, Exed, Jolly, Delicate, Pill, Plus. Angry, Sad, Shifty, Sleepy, Spiral, Squinty, Sultry, Vacant or Starry." +
                "\n- **9.09%** for Extra Short or Extra Tall Height Modifier." },
            { LockboxOption.Slime, "These are the __estimated__ odds taken from 800+ QQQ box openings:\n- **36.97%** for Node Slime Mask. \n- **30.40%** for Node Slime Guards.\n- **10.80%** for Node Container." +
                "\n- **9.47%** for Node Receiver.\n- **3.23%** for Node Slime Crusher.\n- **3.67%** for Node Slime Wall.\n- **1.00%** for Slimed Auras.\n- **1.11%** for Writhing Tendrils.\n- **1.11%** for Early Riser Ring." +
                "\n- **0.67%** for Dawn Bracelet.\n- **0.78%** for Daybreaker Band.\n- **0.33%** for Somnambulist's Totem.\n- **0.45%** for Node Field Aura." },
        };
    }

    private static string GetSlimeboxDescription(string slimeCode)
    {
        return slimeCode switch
        {
            "000" => "The 000 Slime lockbox contains **no special** themed box.",
            "001" => "The 001 Slime lockbox contains **no special** themed box.",
            "002" => "The 002 Slime lockbox contains **no special** themed box.",
            "003" => "The 003 Slime lockbox contains **no special** themed box.",
            "004" => "The 004 Slime lockbox contains **no special** themed box.",
            "005" => "The 005 Slime lockbox contains **no special** themed box.",
            "006" => "The 006 Slime lockbox contains **no special** themed box.",
            "007" => "The 007 Slime lockbox contains **no special** themed box.",
            "008" => "The 008 Slime lockbox contains **no special** themed box.",
            "009" => "The 009 Slime lockbox contains **no special** themed box.",
            "40G" => "The 40G Slime lockbox contains the **Hunter** themed box.",
            "41C" => "The 41C Slime lockbox contains the **Dangerous** themed box.",
            "40N" => "The 40N Slime lockbox contains the **Glacial** themed box.",
            "41D" => "The 41D Slime lockbox contains the **Hazardous** themed box.",
            "50E" => "The 50E Slime lockbox contains the **Wicked** themed box.",
            "509" => "The 509 Slime lockbox contains the **Shadow** themed box.",
            "A1J" => "The A1J Slime lockbox contains the **Pearl** themed box.",
            "A16" => "The A16 Slime lockbox contains the **Opal** themed box.",
            "A1A" => "The A1A Slime lockbox contains the **Amethyst** themed box.",
            "A18" => "The A18 Slime lockbox contains the **Turquoise** themed box.",
            "A10" => "The A10 Slime lockbox contains the **Ruby** themed box.",
            "A12" => "The A12 Slime lockbox contains the **Peridot** themed box.",
            "A1b" => "The A1B Slime lockbox contains **no special** themed box.",
            "403" => "The 403 Slime lockbox contains **no special** themed box.",
            "B1B" => "The B1B Slime lockbox contains the **Aquamarine** themed box.",
            "A17" => "The A17 Slime lockbox contains the **Citrine** themed box.",
            "A19" => "The A19 Slime lockbox contains the **Garnet** themed box.",
            "A14" => "The A14 Slime lockbox contains the **Sapphire** themed box.",
            "A1I" => "The A1I Slime lockbox contains the **Emerald** themed box.",
            "A1H" => "The A1H Slime lockbox contains the **Diamond** themed box.",
            "QQQ" => "The QQQ Slime lockbox contains **no special** themed box.",
            _ => $"I didn't find a match for __{slimeCode}__."
        };
    }

    private string? FindItem(string item)
    {
        item = SpecialCharsRegex().Replace(item, string.Empty);

        var boxOdds = _lockboxes
            .Where(box => SpecialCharsRegex().Replace(box.Value, string.Empty).Contains(item, StringComparison.OrdinalIgnoreCase))
            .Select(box =>
            {
                var boxContent = new System.Text.StringBuilder();
                boxContent.Append($"\n\n__**{box.Key.ToString().ToUpper(CultureInfo.InvariantCulture)} LOCKBOX:**__\n");

                if (box.Key == LockboxOption.Iron)
                {
                    var pools = SpecialCharsRegex().Replace(box.Value, string.Empty).Split("80%");
                    boxContent.Append(pools[0].Contains(item, StringComparison.OrdinalIgnoreCase) ? "**Inside 20% pool:**\n" : "**Inside 80% pool:**\n");
                }

                var matchingLines = box.Value
                    .Split('\n')
                    .Where(line => SpecialCharsRegex().Replace(line, string.Empty).Contains(item, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                for (int i = 0; i < matchingLines.Count; i++)
                {
                    if (box.Key == LockboxOption.Iron && i == 1)
                    {
                        boxContent.Append("**Inside 80% pool:**\n"); // Item appears in both pools => example: "wings"
                    }

                    boxContent.Append($"{matchingLines[i].TrimStart()}\n");
                }

                return boxContent.ToString();
            });

        return string.Join("", boxOdds);
    }

    [GeneratedRegex(@"['""’\+\[\]()\-{},|]")]
    private static partial Regex SpecialCharsRegex();
}
