using Kozma.net.Enums;
using Kozma.net.Models;

namespace Kozma.net.Helpers;

public class BoxHelper(IFileReader jsonFileReader) : IboxHelper
{
    public BoxData? GetBox(Box box)
    {
        return box switch
        {
            Box.Copper => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/f/f2/Usable-Copper_Lockbox_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069744452291264715/Copper.gif",
                "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview#"),
            Box.Steel => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/3/3f/Usable-Steel_Lockbox_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069744452610048120/Steel.gif",
                "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview#"),
            Box.Silver => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/b/bb/Usable-Silver_Lockbox_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069744451938963557/Silver.gif",
                "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview#"),
            Box.Platinum => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/1/1b/Usable-Platinum_Lockbox_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069744453935452191/Platinum.gif",
                "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview#"),
            Box.Gold => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/6/62/Usable-Gold_Lockbox_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069744451586637885/Gold.gif",
                "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview#"),
            Box.Titanium => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/2/2f/Usable-Titanium_Lockbox_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069744454283559033/Titanium.gif",
                "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview#"),
            Box.Iron => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/e/ed/Usable-Iron_Lockbox_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069744453239177399/Iron.gif",
                "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview#"),
            Box.Mirrored => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/8/80/Usable-Mirrored_Lockbox_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069744453562155109/Mirrored.gif",
                "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview#"),
            Box.Slime => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/9/97/Usable-Slime_Lockbox_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069744452937207955/Slime.gif",
                "https://docs.google.com/spreadsheets/d/1f9KQlDcQcoK3K2z6hc7ZTWD_SnrikdTkTXGppneq0YU/htmlview#"),
            Box.Equinox => new BoxData(4.95, BoxCurrency.Dollar,
                "https://media3.spiralknights.com/wiki-images/5/5e/Usable-Equinox_Prize_Box_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069736605075652608/Equinox.gif",
                "https://wiki.spiralknights.com/Equinox_Prize_Box_Promotion_September_2022"),
            Box.Confection => new BoxData(4.95, BoxCurrency.Dollar,
                "https://media3.spiralknights.com/wiki-images/a/a4/Usable-Confection_Prize_Box_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069736605474107462/Confection.gif",
                "https://wiki.spiralknights.com/Confection_Prize_Box_Promotion_August_2014"),
            Box.Spritely => new BoxData(4.95, BoxCurrency.Dollar,
                "https://media3.spiralknights.com/wiki-images/9/90/Usable-Spritely_Prize_Box_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069736604689760276/Spritely.gif",
                "https://wiki.spiralknights.com/Spritely_Prize_Box_Promotion_June_2015"),
            Box.Polar => new BoxData(4.95, BoxCurrency.Dollar,
                "https://media3.spiralknights.com/wiki-images/6/6c/Usable-Polar_Prize_Box_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1074382088016515123/Polar.gif",
                "https://wiki.spiralknights.com/Polar_Prize_Box_Promotion_February_2023"),
            Box.Lucky => new BoxData(3495, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/e/e7/Usable-Lucky_Prize_Box_icon.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069736605822238781/Lucky.gif",
                "https://wiki.spiralknights.com/Lucky_Prize_Box_Promotion_March_2022"),
            _ => null
        };
    }

    public string GetBoxImage(Box box)
    {
        return GetBox(box)?.Url ?? string.Empty;
    }

    public Box? ConvertLockboxOption(LockboxOption box)
    {
        return box switch
        {
            LockboxOption.Copper => Box.Copper,
            LockboxOption.Steel => Box.Steel,
            LockboxOption.Silver => Box.Silver,
            LockboxOption.Platinum => Box.Platinum,
            LockboxOption.Gold => Box.Gold,
            LockboxOption.Titanium => Box.Titanium,
            LockboxOption.Iron => Box.Iron,
            LockboxOption.Mirrored => Box.Mirrored,
            LockboxOption.Slime => Box.Slime,
            _ => null
        };
    }

    public async Task<List<ItemData>> GetItemDataAsync(Box box)
    {
        var projectRoot = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
        if (projectRoot == null) return [];

        var directory = Path.Combine(projectRoot, "Data", "Boxes", $"{box}.json");
        var items = await jsonFileReader.ReadAsync<List<ItemData>>(directory);

        return items ?? [];
    }
}
