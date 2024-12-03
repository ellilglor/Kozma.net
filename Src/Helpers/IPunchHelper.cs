using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;

namespace Kozma.net.Src.Helpers;

public interface IPunchHelper
{
    EmbedAuthorBuilder GetAuthor();
    PunchItem GetItem(PunchOption item);
    PunchOption ConvertToPunchOption(string item);
    Task SendWaitingAnimationAsync(EmbedBuilder embed, SocketInteraction interaction, string url, int delay);
    MessageComponent GetComponents(int uvCount, int lockCount = 0);
    string RollUv(ulong id, PunchItem item, IReadOnlyCollection<string> uvs, bool crafting = false);
    Task<(string desc, string image)> CheckForGmAsync(string user, ItemType type, IReadOnlyCollection<string> uvs);
}
