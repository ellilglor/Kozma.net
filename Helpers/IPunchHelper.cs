using Discord;
using Discord.Interactions;
using Kozma.net.Enums;
using Kozma.net.Models;

namespace Kozma.net.Helpers;

public interface IPunchHelper
{
    public EmbedAuthorBuilder GetAuthor();
    public PunchItem? GetItem(PunchOption item);
    public PunchOption? ConvertToPunchOption(string item);
    public Task SendWaitingAnimationAsync(EmbedBuilder embed, SocketInteractionContext context, string url, int delay);
    public MessageComponent GetComponents(bool lock1, bool lock2, bool lock3, bool gamble1, bool gamble2, bool gamble3);
    public string RollUv(ulong id, PunchItem item, List<string> uvs, bool crafting = false);
    public Task<(string desc, string image)> CheckForGmAsync(ItemType type, List<string> uvs);
}
