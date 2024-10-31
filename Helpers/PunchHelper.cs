using Discord;
using Kozma.net.Enums;
using Kozma.net.Models;

namespace Kozma.net.Helpers;

public class PunchHelper : IPunchHelper
{
    public EmbedAuthorBuilder GetAuthor()
    {
        return new EmbedAuthorBuilder().WithName("Punch").WithIconUrl("https://media3.spiralknights.com/wiki-images/archive/1/1b/20200502113903!Punch-Mugshot.png");
    }

    public PunchItem? GetItem(PunchOption item)
    {
        return item switch
        {
            PunchOption.Brandish => new PunchItem(ItemType.Weapon,
                "https://media3.spiralknights.com/wiki-images/2/22/Brandish-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643184252133406/sword.gif"),
            PunchOption.Mixmaster => new PunchItem(ItemType.Weapon,
                "https://media3.spiralknights.com/wiki-images/f/fd/Overcharged_Mixmaster-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643185170686064/mixmaster.gif"),
            PunchOption.Bomb => new PunchItem(ItemType.Bomb,
                "https://media3.spiralknights.com/wiki-images/c/c2/Blast_Bomb-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643183866253392/bomb.gif"),
            PunchOption.Shield => new PunchItem(ItemType.Shield,
                "https://media3.spiralknights.com/wiki-images/5/5b/Swiftstrike_Buckler-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643184688337027/shield.gif"),
            PunchOption.Helmet => new PunchItem(ItemType.Armor,
                "https://media3.spiralknights.com/wiki-images/2/20/Black_Kat_Cowl-Equipped.png",
                "https://cdn.discordapp.com/attachments/1069643121622777876/1069643185539776532/helm.gif"),
            _ => null
        };
    }

    public string RollUv(ItemType type, List<string> uvs, bool crafting = false)
    {
        throw new NotImplementedException();
    }
}
