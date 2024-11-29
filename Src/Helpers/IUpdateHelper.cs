using Discord;
using Discord.WebSocket;

namespace Kozma.net.Src.Helpers;

public interface IUpdateHelper
{
    public IReadOnlyDictionary<string, ulong> GetChannels();
    public Task<int> UpdateLogsAsync(IMessageChannel channel, int limit = 20, bool reset = false);
}
