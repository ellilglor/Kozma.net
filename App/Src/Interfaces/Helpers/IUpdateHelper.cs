using Discord;
using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Interfaces.Helpers;

public interface IUpdateHelper
{
    IReadOnlyDictionary<string, ulong> GetChannels();
    Task<IReadOnlyCollection<TradeLog>> GetLogsAsync(IMessageChannel channel, int limit = 50);
    void ClearFindLogsCache();
}
