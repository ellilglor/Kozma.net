using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;

namespace Kozma.net.Src.Helpers;

public interface IPunchHelper
{
    EmbedAuthorBuilder GetAuthor();
    Task SendWaitingAnimationAsync(EmbedBuilder embed, SocketInteraction interaction, string url, int delayInMs = 1500);
    MessageComponent GetComponents(int uvCount, int lockCount = 0);
    string RollUv(ulong id, PunchItem item, IReadOnlyCollection<string> uvs, bool crafting = false);
    Task<(string desc, string image)> CheckForGmAsync(string user, ItemType type, IReadOnlyCollection<string> uvs);
}
