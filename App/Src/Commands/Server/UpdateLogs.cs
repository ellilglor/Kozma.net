using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Constants;
using Kozma.net.Src.Interfaces.Handlers;
using Kozma.net.Src.Interfaces.Helpers;
using Kozma.net.Src.Interfaces.Services;
using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
[DefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.KickMembers | GuildPermission.BanMembers)]
public class UpdateLogs(IEmbedHandler embedHandler, IUpdateHelper updateHelper, ITradeLogService tradeLogService) : InteractionModuleBase<SocketInteractionContext>
{
    private sealed record Channel(string Name, IReadOnlyCollection<TradeLog> Logs, string Time);

    [SlashCommand(CommandIds.UpdateLogs, "Kozma's Backpack staff only.")]
    public async Task ExecuteAsync()
    {
        var totalTime = System.Diagnostics.Stopwatch.StartNew();
        var embed = embedHandler.GetEmbed("Executing /update");
        var channels = updateHelper.GetChannels();
        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

        var tasks = channels.Select(async Task<Channel> (channelData) =>
        {
            var (name, id) = channelData;
            if (Context.Guild.GetChannel(id) is not IMessageChannel channel) 
                return new Channel(string.Empty, Array.Empty<TradeLog>(), string.Empty);

            var channelTime = System.Diagnostics.Stopwatch.StartNew();
            var logs = await updateHelper.GetLogsAsync(channel, limit: int.MaxValue);

            channelTime.Stop();
            var elapsed = $"{channelTime.Elapsed.TotalSeconds:F2}";

            await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle($"Finished {name} in {elapsed} seconds.").Build());
            return new Channel(name, logs, elapsed);
        }).ToList();

        var data = (await Task.WhenAll(tasks)).Where(c => !string.IsNullOrEmpty(c.Name)).ToList();
        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle("Uploading logs to database...").Build());

        updateHelper.ClearFindLogsCache();
        await tradeLogService.DeleteAndUpdateLogsAsync(data.SelectMany(c => c.Logs).ToList());

        totalTime.Stop();
        DisplayData(data);
        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle($"Update completed in {totalTime.Elapsed.TotalMinutes:F2} minutes").Build());
    }

    private static void DisplayData(IReadOnlyCollection<Channel> data)
    {
        Console.WriteLine("{0,-20} {1,-10} {2,-10}", "Name", "Count", "Time (s)");
        Console.WriteLine(new string('-', 40));

        foreach (var channel in data)
            Console.WriteLine("{0,-20} {1,-10} {2,-10}", channel.Name, channel.Logs.Count, channel.Time);
    }
}
