using Discord;
using Discord.Interactions;
using Kozma.net.Factories;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Commands.Information;

public class Help : InteractionModuleBase<SocketInteractionContext>, ICommand
{
    private readonly IEmbedFactory _embedFactory;
    private readonly IConfiguration _config;

    public Help(IEmbedFactory embedFactory, IConfigFactory configFactory)
    {
        _embedFactory = embedFactory;
        _config = configFactory.GetConfig();
    }

    [SlashCommand("help", "Explains all commands.")]
    public async Task ExecuteAsync()
    {
        var fields = new List<EmbedFieldBuilder>
        {
            CreateField("/bookchance", "Get the % chance you have of getting at least 1 Book of Dark Rituals.\n`kats` Amount of Black Kats you encountered."),
            CreateField("/clear", "Deletes all the messages the bot has sent you."),
            CreateField("/convert", "Convert your currency. (glorified calculator)\n`amount` Amount you want to convert.\n`rate` Optional custom conversion rate."),
            CreateField("/findlogs", @"Makes the bot search the database for your item.
                `item` Item the bot should look for.\n`months` How far back the bot should search. Default: 6 months.
                `variants` Check for color variants / item family tree. Default: yes.
                `clean` Filter out high value uvs. Default: no.
                `mixed` Check the mixed-trades channel. Default: yes."),
            CreateField("/lockbox", @"Gives information about a lockbox or tells you what box drops your item.
                `boxes` Get the odds from a lockbox.\n`slime` Find where you can find a special themed box.
                `item` Find which lockbox drops your item."),
            CreateField("/punch", "Craft items and roll for Unique Variants without draining your wallet.\n`item` Select the item you want to craft."),
            CreateField("/rate", "Tells you the crowns per energy rate currently in use."),
            CreateField("/unbox", "Simulate opening a box and be disappointed for free.\n`box` Select the box you want to open.")
        };

        var embed = _embedFactory.GetEmbed("Here are all my commands:")
            .WithDescription($"*If you notice a problem please contact <@{_config.GetValue<string>("ids:ownerId")}>*")
            .WithFields(fields)
            .Build();

        var components = new ComponentBuilder()
            .WithButton(label: "Github", url: _config.GetValue<string>("github"), style: ButtonStyle.Link)
            .WithButton(label: "Discord server", url: _config.GetValue<string>("serverInvite"), style: ButtonStyle.Link)
            .Build();

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = components;
        });
    }

    private static EmbedFieldBuilder CreateField(string name, string value)
    {
        return new EmbedFieldBuilder().WithName(name).WithValue(value);
    }
}
