using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Extensions;
using Kozma.net.Src.Models.Entities;
using Kozma.net.Src.Services;
using System.Text.RegularExpressions;

namespace Kozma.net.Src.Helpers;

public partial class UpdateHelper(ITradeLogService tradeLogService) : IUpdateHelper
{
    private static readonly SemaphoreSlim _dbLock = new(1, 1);

    private readonly Dictionary<string, ulong> _channels = new()
    {
        { "special-listings", 807369188133306408 },
        { "2023-flash-sales", 1174278238009294858 },
        { "2022-flash-sales", 1029020424929038386 },
        { "2021-flash-sales", 909112948956483625 },
        { "2020-flash-sales", 782744096167034930 },
        { "mixed-trades", 806452637423370250 },
        { "equipment", 806450782542102529 },
        { "costumes", 806452033291812865 },
        { "helm-top", 806451298434154546 },
        { "helm-front", 806450937380077568 },
        { "helm-back", 806450894693728278 },
        { "helm-side", 806450974029381662 },
        { "armor-front", 806451783383121950 },
        { "armor-back", 806451731826212884 },
        { "armor-rear", 806451814882082819 },
        { "armor-ankle", 806451696322084877 },
        { "armor-aura", 806451662716665878 },
        { "miscellaneous", 806452205146079252 },
        { "Sprite Food", 878045932300677151 },
        { "Materials", 880908641304182785 }
    };

    public IReadOnlyDictionary<string, ulong> GetChannels() => _channels.AsReadOnly();

    public async Task<int> UpdateLogsAsync(SocketTextChannel channel, int limit = 20, bool reset = false)
    {
        var messages = await channel.GetMessagesAsync(limit).FlattenAsync();
        var logs = new List<TradeLog>();

        foreach (var message in messages)
        {
            if (string.IsNullOrEmpty(message.Content)) continue;
            if (!reset && await tradeLogService.CheckIfLogExistsAsync(message.Id)) break;

            logs.Add(ConvertMessage(message, channel.Name));
        }

        if (logs.Count == 0) return 0;

        await _dbLock.WaitAsync();
        try
        {
            await tradeLogService.UpdateLogsAsync(logs, reset, channel.Name);
        }
        finally
        {
            _dbLock.Release();
        }
        return logs.Count;
    }

    private static TradeLog ConvertMessage(IMessage message, string channel)
    {
        var filtered = message.Content.CleanUp();
        var copy = message.Content;
        var date = DateRegex().Match(filtered) is Match match && match.Success ? DateTime.Parse(match.Value) : message.CreatedAt.DateTime;
        if (message.Attachments.Count > 1) copy += "\n\n*This message had multiple images*\n*Click the date to look at them*";

        return new TradeLog()
        {
            Id = message.Id.ToString(),
            Channel = channel,
            Author = message.Author.Username.Contains("Knight Launcher", StringComparison.OrdinalIgnoreCase) ? "Haven Server" : message.Author.Username,
            Date = date,
            MessageUrl = message.GetJumpUrl(),
            Content = filtered,
            OriginalContent = copy,
            Image = message.Attachments.Count > 0 ? message.Attachments.First().Url : null,
        };
    }

    [GeneratedRegex("[0-9]{2}/[0-9]{2}/[0-9]{4}")]
    private static partial Regex DateRegex();
}
