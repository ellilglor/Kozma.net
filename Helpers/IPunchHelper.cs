using Discord;
using Kozma.net.Enums;
using Kozma.net.Models;

namespace Kozma.net.Helpers;

public interface IPunchHelper
{
    public EmbedAuthorBuilder GetAuthor();
    public PunchItem? GetItem(PunchOption item);
    public PunchOption? ConvertToPunchOption(string item);
    public string RollUv(ItemType type, List<string> uvs, bool crafting = false);
}
