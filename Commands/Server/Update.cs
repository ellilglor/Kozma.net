using Discord.Interactions;
using Discord;
using Kozma.net.Factories;
using Discord.WebSocket;
using Kozma.net.Models;
using System.Text.RegularExpressions;
using Kozma.net.Helpers;
using Kozma.net.Services;

namespace Kozma.net.Commands.Server;

public class Update(IEmbedFactory embedFactory, IContentHelper contentHelper, ITradeLogService tradeLogService) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Dictionary<string, ulong> Channels = new()
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

    [SlashCommand("update", "Kozma's Backpack staff only.")]
    [RequireUserPermission(GuildPermission.BanMembers | GuildPermission.KickMembers)]
    public async Task ExecuteAsync()
    {
        var totalTime = System.Diagnostics.Stopwatch.StartNew();
        var embed = embedFactory.GetEmbed("Executing /update");
        var data = new List<Channel>();
        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

        foreach (var (name, id) in Channels)
        {
            if (Context.Guild.GetChannel(id) is not SocketTextChannel channel) continue;
            var channelTime = System.Diagnostics.Stopwatch.StartNew();
            var count = await UpdateLogsAsync(channel, reset: true);

            channelTime.Stop();
            var elapsed = $"{channelTime.Elapsed.TotalSeconds:F2}";
            data.Add(new Channel(name, count, elapsed));
            await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle($"Finished {name} in {elapsed} seconds").Build());
        }

        totalTime.Stop();
        DisplayData(data);
        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle($"Update completed in {totalTime.Elapsed.TotalMinutes:F2} minutes").Build());
    }

    private async Task<int> UpdateLogsAsync(SocketTextChannel channel, bool reset = false)
    {
        var messages = await channel.GetMessagesAsync(int.MaxValue).FlattenAsync();
        var logs = new List<TradeLog>();

        foreach (var message in messages)
        {
            if (string.IsNullOrEmpty(message.Content)) continue;
            logs.Add(ConvertMessage(message, channel.Name));
        }

        await tradeLogService.UpdateLogsAsync(logs, reset, channel.Name);
        return logs.Count;
    }

    private TradeLog ConvertMessage(IMessage message, string channel)
    {
        var pattern = @"[0-9]{2}/[0-9]{2}/[0-9]{4}";
        var filtered = contentHelper.FilterContent(message.Content);
        var copy = message.Content;
        var date = Regex.Match(filtered, pattern) is Match match && match.Success ? DateTime.Parse(match.Value) : message.CreatedAt.DateTime;
        if (message.Attachments.Count > 1) copy += "\n\n*This message had multiple images*\n*Click the date to look at them*";

        return new TradeLog()
        {
            Id = message.Id.ToString(),
            Channel = channel,
            Author = message.Author.Username.Contains("Knight Launcher") ? "Haven Server" : message.Author.Username,
            Date = date,
            MessageUrl = message.GetJumpUrl(),
            Content = filtered,
            OriginalContent = copy,
            Image = message.Attachments.Count > 0 ? message.Attachments.First().Url : null,
        };
    }

    private static void DisplayData(List<Channel> data)
    {
        Console.WriteLine("{0,-20} {1,-10} {2,-10}", "Name", "Count", "Time (s)");

        Console.WriteLine(new string('-', 40));

        foreach (var channel in data)
        {
            Console.WriteLine("{0,-20} {1,-10} {2,-10}", channel.Name, channel.Count, channel.Time);
        }
    }
}
