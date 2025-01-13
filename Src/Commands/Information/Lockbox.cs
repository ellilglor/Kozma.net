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
    private readonly Dictionary<LockboxOption, string> _lockboxes = new()
    {
        { LockboxOption.Colors, $"- {Format.Bold("97.85%")} for Cool, Regal, Military, Heavy, Fancy, Dusky or Toasty.\n- {Format.Bold("1.96%")} for Divine or Volcanic\n- {Format.Bold("0.19%")} for Prismatic." },
        { LockboxOption.Copper, $"- {Format.Bold("1.92%")} for a Shadow Key.\n- {Format.Bold("76.78%")} for Binoculars, Flower, Headband or Plume.\n- {Format.Bold("19.19%")} for Long Feather or Pipe." +
            $"\n- {Format.Bold("3.84%")} for Wolver tail or Prismatic glow-eyes.\n- {Format.Bold("0.19%")} for Twinkle Aura or Twilight Aura." },
        { LockboxOption.Steel, $"- {Format.Bold("1.92%")} for a Shadow Key.\n- {Format.Bold("76.78%")} for Bolted Vee, Wide Vee, Mecha Wings or Game Face.\n- {Format.Bold("19.19%")} for Vertical Vents or Spike Mohawk." +
            $"\n- {Format.Bold("3.84%")} for Ankle Booster or Aero Fin.\n- {Format.Bold("0.19%")} for Shoulder Booster or Flame Aura." },
        { LockboxOption.Silver, $"- {Format.Bold("1.92%")} for a Shadow Key.\n- {Format.Bold("76.78%")} for Vitakit, Targeting Module, Binocular Visor or Helm-Mounted Display.\n- {Format.Bold("19.19%")} for Maedate or Intel Tube." +
            $"\n- {Format.Bold("3.84%")} for Giga Shades.\n- {Format.Bold("0.19%")} for Wings (50%) or Divine/Volcanic/Prismatic Halo (50%)." },
        { LockboxOption.Platinum, $"- {Format.Bold("1.92%")} for a Shadow Key.\n- {Format.Bold("76.78%")} for Com Unit, Knight Vision Goggles or Goggles.\n- {Format.Bold("19.19%")} for Sensor Unit or Bomb Bandolier." +
            $"\n- {Format.Bold("3.84%")} for Mohawk, Devious Horns or Scarf.\n- {Format.Bold("0.19%")} for Unclean Aura or Ghostly Aura." },
        { LockboxOption.Gold, $"- {Format.Bold("1.92%")} for a Shadow Key.\n- {Format.Bold("76.78%")} for Canteen, Ribbon or Maid Headband.\n- {Format.Bold("19.19%")} for Monocle or Glasses." +
            $"\n- {Format.Bold("3.84%")} for Mustache or Round Shades.\n- {Format.Bold("0.19%")} for Dapper Combo or Toupee." },
        { LockboxOption.Titanium, $"- {Format.Bold("1.92%")} for a Shadow Key.\n- {Format.Bold("76.78%")} for Helm Guards, Munitions Pack, Barrel Belly or Vented Visor.\n- {Format.Bold("19.19%")} for Headlamp or Side Blade." +
            $"\n- {Format.Bold("3.84%")} for Rebreather or Parrying Blade.\n- {Format.Bold("0.19%")} for Vial Bandolier." },
        { LockboxOption.Iron, $"- {Format.Bold("1.92%")} for a Shadow Key.\n### 20% for one of the following:\n- {Format.Bold("76.78%")} for Vitakit, Canteen or Barrel Belly." +
            $"\n- {Format.Bold("19.19%")} for Side Blade or Bomb Bandolier.\n- {Format.Bold("3.84%")} for Wolver Tail or Parrying Blade." +
            $"\n- {Format.Bold("0.19%")} with {Format.Bold("50%")} for Wings and {Format.Bold("50%")} to get an Aura: {Format.Bold("27.78%")} for Twinkle, Ghostly and Unclean. {Format.Bold("13.89%")} for Twilight & {Format.Bold("2.77%")} for Flame." +
            $"\n### 80% for one of the following:\n- {Format.Bold("76.78%")} for Plume, Ribbon, Vented Visor, Binocular Visor, Knight Vision Goggles, Helm-Mounted Display, Goggles, Com Unit, Mecha Wings, Helm Guards, Bolted Vee, Headband, Wide Vee, Maid Headband or Flower." +
            $"\n- {Format.Bold("19.19%")} for Long Feather, Vertical Vents, Pipe, Glasses or Maedate.\n- {Format.Bold("3.84%")} with {Format.Bold("24.39%")} for Scarf, Mustache and Mohawk and {Format.Bold("2.44%")} for Prismatic Glow-Eyes." +
            $"\n- {Format.Bold("0.19%")} with {Format.Bold("43.48%")} for Dapper Combo and Toupee & {Format.Bold("13.04%")} for Divine/Volcanic/Prismatic Halo." },
        { LockboxOption.Mirrored, $"- {Format.Bold("90.91%")} for the following eyes:\n- Cheeky, Closed, Dot, Exed, Jolly, Delicate, Pill, Plus. Angry, Sad, Shifty, Sleepy, Spiral, Squinty, Sultry, Vacant or Starry." +
            $"\n- {Format.Bold("9.09%")} for Extra Short or Extra Tall Height Modifier." },
        { LockboxOption.Slime, $"These are the {Format.Underline("estimated")} odds taken from 800+ QQQ box openings:\n- {Format.Bold("36.97%")} for Node Slime Mask. \n- {Format.Bold("30.40%")} for Node Slime Guards.\n- {Format.Bold("10.80%")} for Node Container." +
            $"\n- {Format.Bold("9.47%")} for Node Receiver.\n- {Format.Bold("3.23%")} for Node Slime Crusher.\n- {Format.Bold("3.67%")} for Node Slime Wall.\n- {Format.Bold("1.00%")} for Slimed Auras.\n- {Format.Bold("1.11%")} for Writhing Tendrils." +
            $"\n- {Format.Bold("1.11%")} for Early Riser Ring.\n- {Format.Bold("0.67%")} for Dawn Bracelet.\n- {Format.Bold("0.78%")} for Daybreaker Band.\n- {Format.Bold("0.33%")} for Somnambulist's Totem.\n- {Format.Bold("0.45%")} for Node Field Aura." },
    };

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

            embed.WithTitle(string.IsNullOrEmpty(desc) ? $"I didn't find a box containing {Format.Underline(item)}." : $"These lockboxes contain {Format.Underline(item)}:")
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

    private static string GetSlimeboxDescription(string slimeCode)
    {
        return slimeCode switch
        {
            "000" => SlimeboxMatch(slimeCode, "no special", false),
            "001" => SlimeboxMatch(slimeCode, "no special", false),
            "002" => SlimeboxMatch(slimeCode, "no special", false),
            "003" => SlimeboxMatch(slimeCode, "no special", false),
            "004" => SlimeboxMatch(slimeCode, "no special", false),
            "005" => SlimeboxMatch(slimeCode, "no special", false),
            "006" => SlimeboxMatch(slimeCode, "no special", false),
            "007" => SlimeboxMatch(slimeCode, "no special", false),
            "008" => SlimeboxMatch(slimeCode, "no special", false),
            "009" => SlimeboxMatch(slimeCode, "no special", false),
            "40G" => SlimeboxMatch(slimeCode, "Hunter"),
            "41C" => SlimeboxMatch(slimeCode, "Dangerous"),
            "40N" => SlimeboxMatch(slimeCode, "Glacial"),
            "41D" => SlimeboxMatch(slimeCode, "Hazardous"),
            "50E" => SlimeboxMatch(slimeCode, "Wicked"),
            "509" => SlimeboxMatch(slimeCode, "Shadow"),
            "A1J" => SlimeboxMatch(slimeCode, "Pearl"),
            "A16" => SlimeboxMatch(slimeCode, "Opal"),
            "A1A" => SlimeboxMatch(slimeCode, "Amethyst"),
            "A18" => SlimeboxMatch(slimeCode, "Turquoise"),
            "A10" => SlimeboxMatch(slimeCode, "Ruby"),
            "A12" => SlimeboxMatch(slimeCode, "Peridot"),
            "A1B" => SlimeboxMatch(slimeCode, "no special", false),
            "403" => SlimeboxMatch(slimeCode, "no special", false),
            "B1B" => SlimeboxMatch(slimeCode, "Aquamarine"),
            "A17" => SlimeboxMatch(slimeCode, "Citrine"),
            "A19" => SlimeboxMatch(slimeCode, "Garnet"),
            "A14" => SlimeboxMatch(slimeCode, "Sapphire"),
            "A1I" => SlimeboxMatch(slimeCode, "Emerald"),
            "A1H" => SlimeboxMatch(slimeCode, "Diamond"),
            "QQQ" => SlimeboxMatch(slimeCode, "no special", false),
            _ => $"I didn't find a match for {Format.Underline(slimeCode)}."
        };
    }

    private static string SlimeboxMatch(string code, string content, bool hasSpecial = true) =>
        $"The {code} Slime lockbox contains {(hasSpecial ? "the " : string.Empty)}{Format.Underline(content)} themed box.";

    private string? FindItem(string item)
    {
        item = SpecialCharsRegex().Replace(item, string.Empty);

        var boxOdds = _lockboxes
            .Where(box => SpecialCharsRegex().Replace(box.Value, string.Empty).Contains(item, StringComparison.OrdinalIgnoreCase))
            .Select(box =>
            {
                var boxContent = new System.Text.StringBuilder();
                boxContent.Append($"\n\n{Format.Underline(Format.Bold(box.Key.ToString().ToUpper(CultureInfo.InvariantCulture) + " LOCKBOX:"))}\n");

                if (box.Key == LockboxOption.Iron)
                {
                    var pools = SpecialCharsRegex().Replace(box.Value, string.Empty).Split("80%");
                    boxContent.Append(Format.Bold(pools[0].Contains(item, StringComparison.OrdinalIgnoreCase) ? "Inside 20% pool:" : "Inside 80% pool:") + "\n");
                }

                var matchingLines = box.Value
                    .Split('\n')
                    .Where(line => SpecialCharsRegex().Replace(line, string.Empty).Contains(item, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                for (int i = 0; i < matchingLines.Count; i++)
                {
                    if (box.Key == LockboxOption.Iron && i == 1)
                    {
                        boxContent.Append(Format.Bold("Inside 80% pool:") + "\n"); // Item appears in both pools => example: "wings"
                    }

                    boxContent.Append($"{matchingLines[i].TrimStart()}\n");
                }

                return boxContent.ToString();
            });

        return string.Join(string.Empty, boxOdds);
    }

    [GeneratedRegex(@"['""’\+\[\]()\-{},|]")]
    private static partial Regex SpecialCharsRegex();
}
