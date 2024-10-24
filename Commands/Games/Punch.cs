using Discord.Interactions;
using Kozma.net.Enums;
using Kozma.net.Factories;
using Kozma.net.Helpers;

namespace Kozma.net.Commands.Games;

public class Punch(IEmbedFactory embedFactory, IboxHelper boxHelper) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("unbox", "Simulate opening a Prize Box or Lockbox.")]
    public async Task ExecuteAsync(
        [Summary(name: "box", description: "Select the box you want to open.")] Box box)
    {
        await UnboxAsync(box);
    }

    public async Task UnboxAsync(Box box)
    {
        var boxData = boxHelper.GetBox(box);
        var finalEmbed = embedFactory.GetEmbed(string.Empty);
        var test = await OpenAsync(box);

        await ModifyOriginalResponseAsync(msg => msg.Embed = finalEmbed.Build());
    }

    private async Task<List<string>> OpenAsync(Box box)
    {
        var items = await boxHelper.GetItemDataAsync(box);

        return new List<string>();
    }
}
