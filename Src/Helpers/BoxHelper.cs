using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;

namespace Kozma.net.Src.Helpers;

public class BoxHelper(IFileReader jsonFileReader) : IBoxHelper
{
    private static readonly string _lockboxSheetUrl = "https://docs.google.com/spreadsheets/d/14FQWsNevL-7Uiiy-Q3brif8FaEaH7zGGR2Lv_JkOyr8/htmlview#";
    private static readonly string _baseGifUrl = "https://cdn.discordapp.com/attachments/1069643121622777876/";

    public BoxData? GetBox(Box box)
    {
        return box switch
        {
            Box.Copper => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/f/f2/Usable-Copper_Lockbox_icon.png",
                $"{_baseGifUrl}1069744452291264715/Copper.gif",
                _lockboxSheetUrl),
            Box.Steel => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/3/3f/Usable-Steel_Lockbox_icon.png",
                 $"{_baseGifUrl}1069744452610048120/Steel.gif",
                _lockboxSheetUrl),
            Box.Silver => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/b/bb/Usable-Silver_Lockbox_icon.png",
                 $"{_baseGifUrl}1069744451938963557/Silver.gif",
                _lockboxSheetUrl),
            Box.Platinum => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/1/1b/Usable-Platinum_Lockbox_icon.png",
                 $"{_baseGifUrl}1069744453935452191/Platinum.gif",
                _lockboxSheetUrl),
            Box.Gold => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/6/62/Usable-Gold_Lockbox_icon.png",
                 $"{_baseGifUrl}1069744451586637885/Gold.gif",
                _lockboxSheetUrl),
            Box.Titanium => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/2/2f/Usable-Titanium_Lockbox_icon.png",
                 $"{_baseGifUrl}1069744454283559033/Titanium.gif",
                _lockboxSheetUrl),
            Box.Iron => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/e/ed/Usable-Iron_Lockbox_icon.png",
                $"{_baseGifUrl}1069744453239177399/Iron.gif",
                _lockboxSheetUrl),
            Box.Mirrored => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/8/80/Usable-Mirrored_Lockbox_icon.png",
                 $"{_baseGifUrl}1069744453562155109/Mirrored.gif",
                _lockboxSheetUrl),
            Box.Slime => new BoxData(750, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/9/97/Usable-Slime_Lockbox_icon.png",
                 $"{_baseGifUrl}1069744452937207955/Slime.gif",
                "https://docs.google.com/spreadsheets/d/1f9KQlDcQcoK3K2z6hc7ZTWD_SnrikdTkTXGppneq0YU/htmlview#"),
            Box.Equinox => new BoxData(4.95, BoxCurrency.Dollar,
                "https://media3.spiralknights.com/wiki-images/5/5e/Usable-Equinox_Prize_Box_icon.png",
                 $"{_baseGifUrl}1069736605075652608/Equinox.gif",
                "https://wiki.spiralknights.com/Equinox_Prize_Box_Promotion_September_2022"),
            Box.Confection => new BoxData(4.95, BoxCurrency.Dollar,
                "https://media3.spiralknights.com/wiki-images/a/a4/Usable-Confection_Prize_Box_icon.png",
                $"{_baseGifUrl}1069736605474107462/Confection.gif",
                "https://wiki.spiralknights.com/Confection_Prize_Box_Promotion_August_2014"),
            Box.Spritely => new BoxData(4.95, BoxCurrency.Dollar,
                "https://media3.spiralknights.com/wiki-images/9/90/Usable-Spritely_Prize_Box_icon.png",
                 $"{_baseGifUrl}1069736604689760276/Spritely.gif",
                "https://wiki.spiralknights.com/Spritely_Prize_Box_Promotion_June_2015"),
            Box.Polar => new BoxData(4.95, BoxCurrency.Dollar,
                "https://media3.spiralknights.com/wiki-images/6/6c/Usable-Polar_Prize_Box_icon.png",
                 $"{_baseGifUrl}1074382088016515123/Polar.gif",
                "https://wiki.spiralknights.com/Polar_Prize_Box_Promotion_February_2023"),
            Box.Lucky => new BoxData(3495, BoxCurrency.Energy,
                "https://media3.spiralknights.com/wiki-images/e/e7/Usable-Lucky_Prize_Box_icon.png",
                 $"{_baseGifUrl}1069736605822238781/Lucky.gif",
                "https://wiki.spiralknights.com/Lucky_Prize_Box_Promotion_March_2022"),
            _ => null
        };
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

    public double CalculateCost(int amount, BoxData box)
    {
        switch (box.Currency)
        {
            case BoxCurrency.Energy: return amount * box.Price;
            case BoxCurrency.Dollar:
                var cost = amount / 14 * 49.95;
                amount %= 14;

                cost += amount / 5 * 19.95;
                amount %= 5;

                cost += amount * 4.95;
                return Math.Round(cost, 2);
            default: return box.Price;
        }
    }

    public async Task<List<ItemData>> GetItemDataAsync(Box box)
    {
        return await jsonFileReader.ReadAsync<List<ItemData>>(Path.Combine("Data", "Boxes", $"{box}.json")) ?? [];
    }
}
