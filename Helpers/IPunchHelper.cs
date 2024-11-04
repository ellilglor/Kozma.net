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
    public MessageComponent GetComponents(bool lock1, bool lock2, bool lock3);
    public string RollUv(ItemType type, List<string> uvs, bool crafting = false);
}
