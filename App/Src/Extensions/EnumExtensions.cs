using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;

namespace Kozma.net.Src.Extensions;

public static class EnumExtensions
{
    // Box extensions
    private const string LockboxSheetUrl = "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview#";
    private const string BaseGifUrl = "https://cdn.discordapp.com/attachments/1069643121622777876/";

    private static readonly Dictionary<Box, BoxData> BoxDataMapping = new()
    {
        { Box.Copper, new BoxData(750, BoxCurrency.Energy, "https://media3.spiralknights.com/wiki-images/f/f2/Usable-Copper_Lockbox_icon.png", $"{BaseGifUrl}1069744452291264715/Copper.gif", LockboxSheetUrl) },
        { Box.Steel, new BoxData(750, BoxCurrency.Energy, "https://media3.spiralknights.com/wiki-images/3/3f/Usable-Steel_Lockbox_icon.png", $"{BaseGifUrl}1069744452610048120/Steel.gif", LockboxSheetUrl) },
        { Box.Silver, new BoxData(750, BoxCurrency.Energy, "https://media3.spiralknights.com/wiki-images/b/bb/Usable-Silver_Lockbox_icon.png", $"{BaseGifUrl}1069744451938963557/Silver.gif", LockboxSheetUrl) },
        { Box.Platinum, new BoxData(750, BoxCurrency.Energy, "https://media3.spiralknights.com/wiki-images/1/1b/Usable-Platinum_Lockbox_icon.png", $"{BaseGifUrl}1069744453935452191/Platinum.gif", LockboxSheetUrl) },
        { Box.Gold, new BoxData(750, BoxCurrency.Energy, "https://media3.spiralknights.com/wiki-images/6/62/Usable-Gold_Lockbox_icon.png", $"{BaseGifUrl}1069744451586637885/Gold.gif", LockboxSheetUrl) },
        { Box.Titanium, new BoxData(750, BoxCurrency.Energy, "https://media3.spiralknights.com/wiki-images/2/2f/Usable-Titanium_Lockbox_icon.png", $"{BaseGifUrl}1069744454283559033/Titanium.gif", LockboxSheetUrl) },
        { Box.Iron, new BoxData(750, BoxCurrency.Energy, "https://media3.spiralknights.com/wiki-images/e/ed/Usable-Iron_Lockbox_icon.png", $"{BaseGifUrl}1069744453239177399/Iron.gif", LockboxSheetUrl) },
        { Box.Mirrored, new BoxData(750, BoxCurrency.Energy, "https://media3.spiralknights.com/wiki-images/8/80/Usable-Mirrored_Lockbox_icon.png", $"{BaseGifUrl}1069744453562155109/Mirrored.gif", LockboxSheetUrl) },
        { Box.Slime, new BoxData(750, BoxCurrency.Energy, "https://media3.spiralknights.com/wiki-images/9/97/Usable-Slime_Lockbox_icon.png", $"{BaseGifUrl}1069744452937207955/Slime.gif", "https://docs.google.com/spreadsheets/d/1f9KQlDcQcoK3K2z6hc7ZTWD_SnrikdTkTXGppneq0YU/htmlview#") },
        { Box.Equinox, new BoxData(4.95, BoxCurrency.Dollar, "https://media3.spiralknights.com/wiki-images/5/5e/Usable-Equinox_Prize_Box_icon.png", $"{BaseGifUrl}1069736605075652608/Equinox.gif", "https://wiki.spiralknights.com/Equinox_Prize_Box_Promotion_September_2022") },
        { Box.Confection, new BoxData(4.95, BoxCurrency.Dollar, "https://media3.spiralknights.com/wiki-images/a/a4/Usable-Confection_Prize_Box_icon.png", $"{BaseGifUrl}1069736605474107462/Confection.gif", "https://wiki.spiralknights.com/Confection_Prize_Box_Promotion_August_2014") },
        { Box.Spritely, new BoxData(4.95, BoxCurrency.Dollar, "https://media3.spiralknights.com/wiki-images/9/90/Usable-Spritely_Prize_Box_icon.png", $"{BaseGifUrl}1069736604689760276/Spritely.gif", "https://wiki.spiralknights.com/Spritely_Prize_Box_Promotion_June_2015") },
        { Box.Polar, new BoxData(4.95, BoxCurrency.Dollar, "https://media3.spiralknights.com/wiki-images/6/6c/Usable-Polar_Prize_Box_icon.png", $"{BaseGifUrl}1074382088016515123/Polar.gif", "https://wiki.spiralknights.com/Polar_Prize_Box_Promotion_February_2023") },
        { Box.Lucky, new BoxData(3495, BoxCurrency.Energy, "https://media3.spiralknights.com/wiki-images/e/e7/Usable-Lucky_Prize_Box_icon.png", $"{BaseGifUrl}1069736605822238781/Lucky.gif", "https://wiki.spiralknights.com/Lucky_Prize_Box_Promotion_March_2022") },
    };

    public static BoxData ToBoxData(this Box box) =>
        BoxDataMapping.TryGetValue(box, out var data) ? data : throw new  InvalidCastException($"Box '{box}' is not a valid box type.");

    // Lockbox extensions
    private static readonly Dictionary<LockboxOption, Box> LockboxOptionMapping = new()
    {
        { LockboxOption.Copper, Box.Copper },
        { LockboxOption.Steel, Box.Steel },
        { LockboxOption.Silver, Box.Silver },
        { LockboxOption.Platinum, Box.Platinum },
        { LockboxOption.Gold, Box.Gold },
        { LockboxOption.Titanium, Box.Titanium },
        { LockboxOption.Iron, Box.Iron },
        { LockboxOption.Mirrored, Box.Mirrored },
        { LockboxOption.Slime, Box.Slime },
    };

    public static Box ConvertToBox(this LockboxOption box) =>
        LockboxOptionMapping.TryGetValue(box, out var data) ? data : throw new InvalidCastException($"Lockbox '{box}' is not a valid lockbox type.");

    // PunchOption extensions
    private static readonly Dictionary<PunchOption, PunchItem> PunchOptionMapping = new()
    {
        { PunchOption.Brandish, new PunchItem("Brandish", ItemType.Weapon, "https://media3.spiralknights.com/wiki-images/2/22/Brandish-Equipped.png",
            "https://cdn.discordapp.com/attachments/1069643121622777876/1069643184252133406/sword.gif") },
        { PunchOption.Mixmaster, new PunchItem("Overcharged Mixmaster", ItemType.Weapon, "https://media3.spiralknights.com/wiki-images/f/fd/Overcharged_Mixmaster-Equipped.png",
            "https://cdn.discordapp.com/attachments/1069643121622777876/1069643185170686064/mixmaster.gif") },
        { PunchOption.Bomb, new PunchItem("Blast Bomb", ItemType.Bomb, "https://media3.spiralknights.com/wiki-images/c/c2/Blast_Bomb-Equipped.png",
            "https://cdn.discordapp.com/attachments/1069643121622777876/1069643183866253392/bomb.gif") },
        { PunchOption.Shield, new PunchItem("Swiftstrike Buckler", ItemType.Shield, "https://media3.spiralknights.com/wiki-images/5/5b/Swiftstrike_Buckler-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643184688337027/shield.gif") },
        { PunchOption.Helmet, new PunchItem("Black Kat Cowl", ItemType.Armor, "https://media3.spiralknights.com/wiki-images/2/20/Black_Kat_Cowl-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643185539776532/helm.gif") }
    };

    public static PunchItem ToPunchItem(this PunchOption item) =>
        PunchOptionMapping.TryGetValue(item, out var data) ? data : throw new InvalidCastException($"{item} is not a valid PunchOption");

    // Logcolor extensions
    private static readonly Dictionary<LogLevel, string> LogLevelMapping = new()
    {
        { LogLevel.Command, "\u001b[34m" },
        { LogLevel.Button, "\u001b[36m" },
        { LogLevel.Moderation, "\u001b[35m" },
        { LogLevel.Info, "\u001b[33m" },
        { LogLevel.Discord, "\u001b[90m" },
        { LogLevel.Special, "\u001b[32m" },
        { LogLevel.Error, "\u001b[31m" }
    };

    public static string Color(this LogLevel level) =>
        LogLevelMapping.TryGetValue(level, out var color) ? color : "\u001b[37m";
}
