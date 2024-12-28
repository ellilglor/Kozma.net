using Discord;
using Discord.Interactions;
using Kozma.net.Src.Handlers;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Models.Entities;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
[DefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.KickMembers | GuildPermission.BanMembers)]
public class UpdateLogs(IEmbedHandler embedHandler, IUpdateHelper updateHelper) : InteractionModuleBase<SocketInteractionContext>
{
    private sealed record Channel(string Name, IReadOnlyCollection<TradeLog> Logs, string Time);

    [SlashCommand("update", "Kozma's Backpack staff only.")]
    public async Task ExecuteAsync()
    {
        var totalTime = System.Diagnostics.Stopwatch.StartNew();
        var embed = embedHandler.GetEmbed("Executing /update");
        var data = new List<Channel>();
        var channels = updateHelper.GetChannels();
        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());

        var tasks = channels.Select(async (channelData) =>
        {
            var (name, id) = channelData;
            if (Context.Guild.GetChannel(id) is not IMessageChannel channel) return;
            var channelTime = System.Diagnostics.Stopwatch.StartNew();
            var logs = await updateHelper.GetLogsAsync(channel, limit: int.MaxValue);

            channelTime.Stop();
            var elapsed = $"{channelTime.Elapsed.TotalSeconds:F2}";
            data.Add(new Channel(name, logs, elapsed));

            await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle($"Finished {name} in {elapsed} seconds").Build());
        }).ToList();

        await Task.WhenAll(tasks);

        foreach (var channel in data)
        {
            await updateHelper.UpdateLogsAsync(channel.Logs, reset: true, channel: channel.Name);
        }

        totalTime.Stop();
        DisplayData(data);
        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle($"Update completed in {totalTime.Elapsed.TotalMinutes:F2} minutes").Build());
    }

    private static void DisplayData(IReadOnlyCollection<Channel> data)
    {
        Console.WriteLine("{0,-20} {1,-10} {2,-10}", "Name", "Count", "Time (s)");

        Console.WriteLine(new string('-', 40));

        foreach (var channel in data)
        {
            Console.WriteLine("{0,-20} {1,-10} {2,-10}", channel.Name, channel.Logs.Count, channel.Time);
        }
    }
}
