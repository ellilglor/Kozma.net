using Discord;
using Discord.Interactions;
using Kozma.net.Handlers;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Commands.Information;

public class Help(IEmbedHandler embedHandler, IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("help", "Explains all commands.")]
    public async Task ExecuteAsync()
    {
        var inline = false;
        var fields = new List<EmbedFieldBuilder>
        {
            embedHandler.CreateField("/bookchance", "Get the % chance you have of getting at least 1 Book of Dark Rituals.\n`kats` Amount of Black Kats you encountered.", inline),
            embedHandler.CreateField("/clear", "Deletes all the messages the bot has sent you.", inline),
            embedHandler.CreateField("/convert", "Convert your currency. (glorified calculator)\n`amount` Amount you want to convert.\n`rate` Optional custom conversion rate.", inline),
            embedHandler.CreateField("/findlogs", @"Makes the bot search the database for your item.
                `item` Item the bot should look for.
                `months` How far back the bot should search. Default: 6 months.
                `variants` Check for color variants / item family tree. Default: yes.
                `clean` Filter out high value uvs. Default: no.
                `mixed` Check the mixed-trades channel. Default: yes.", inline),
            embedHandler.CreateField("/lockbox", @"Gives information about a lockbox or tells you what box drops your item.
                `boxes` Get the odds from a lockbox.
                `slime` Find where you can find a special themed box.
                `item` Find which lockbox drops your item.", inline),
            embedHandler.CreateField("/punch", "Craft items and roll for Unique Variants without draining your wallet.\n`item` Select the item you want to craft.", inline),
            embedHandler.CreateField("/rate", "Tells you the crowns per energy rate currently in use.", inline),
            embedHandler.CreateField("/unbox", "Simulate opening a box and be disappointed for free.\n`box` Select the box you want to open.", inline)
        };

        var embed = embedHandler.GetEmbed("Here are all my commands:")
            .WithDescription($"*If you notice a problem please contact <@{config.GetValue<string>("ids:ownerId")}>*")
            .WithFields(fields)
            .Build();

        var components = new ComponentBuilder()
            .WithButton(label: "Github", url: config.GetValue<string>("github"), style: ButtonStyle.Link)
            .WithButton(label: "Discord server", url: config.GetValue<string>("serverInvite"), style: ButtonStyle.Link)
            .Build();

        await ModifyOriginalResponseAsync(msg => {
            msg.Embed = embed;
            msg.Components = components;
        });
    }
}
