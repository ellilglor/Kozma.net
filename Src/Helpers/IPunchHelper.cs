using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Enums;
using Kozma.net.Src.Models;

namespace Kozma.net.Src.Helpers;

public interface IPunchHelper
{
    public EmbedAuthorBuilder GetAuthor();
    public PunchItem GetItem(PunchOption item);
    public PunchOption ConvertToPunchOption(string item);
    public Task SendWaitingAnimationAsync(EmbedBuilder embed, SocketInteraction interaction, string url, int delay);
    public MessageComponent GetComponents(int uvCount, int lockCount = 0);
    public string RollUv(ulong id, PunchItem item, IReadOnlyCollection<string> uvs, bool crafting = false);
    public Task<(string desc, string image)> CheckForGmAsync(string user, ItemType type, IReadOnlyCollection<string> uvs);
}
