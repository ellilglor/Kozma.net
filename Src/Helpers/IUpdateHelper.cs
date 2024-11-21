using Discord.WebSocket;

namespace Kozma.net.Src.Helpers;

public interface IUpdateHelper
{
    public Dictionary<string, ulong> GetChannels();
    public Task<int> UpdateLogsAsync(SocketTextChannel channel, int limit = 20, bool reset = false);
}
