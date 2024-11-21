using Discord.Interactions;
using Discord;
using Discord.WebSocket;
using Kozma.net.Src.Helpers;
using Kozma.net.Src.Handlers;

namespace Kozma.net.Src.Commands.Server;

public class Update(IEmbedHandler embedHandler, IUpdateHelper updateHelper) : InteractionModuleBase<SocketInteractionContext>
{
    private record Channel(string Name, int Count, string Time);

    [SlashCommand("update", "Kozma's Backpack staff only.")]
    [RequireUserPermission(GuildPermission.BanMembers | GuildPermission.KickMembers)]
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
            if (Context.Guild.GetChannel(id) is not SocketTextChannel channel) return;
            var channelTime = System.Diagnostics.Stopwatch.StartNew();
            var count = await updateHelper.UpdateLogsAsync(channel, limit: int.MaxValue, reset: true);

            channelTime.Stop();
            var elapsed = $"{channelTime.Elapsed.TotalSeconds:F2}";
            data.Add(new Channel(name, count, elapsed));

            await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle($"Finished {name} in {elapsed} seconds").Build());
        }).ToList();

        await Task.WhenAll(tasks);

        totalTime.Stop();
        DisplayData(data);
        await ModifyOriginalResponseAsync(msg => msg.Embed = embed.WithTitle($"Update completed in {totalTime.Elapsed.TotalMinutes:F2} minutes").Build());
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
