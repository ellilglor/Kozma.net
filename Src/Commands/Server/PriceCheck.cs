﻿using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;

namespace Kozma.net.Src.Commands.Server;

[DontAutoRegister]
public class PriceCheck(IConfiguration config) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("pricecheck", "Kozma's Backpack staff only.")]
    [RequireUserPermission(GuildPermission.BanMembers | GuildPermission.KickMembers)]
    public async Task ExecuteAsync()
    {
        var projectRoot = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName ?? throw new DirectoryNotFoundException($"Failed to find projectRoot for pricecheck image");

        await Context.Channel.SendFileAsync(
            filePath: Path.Combine(projectRoot, "Src", "Assets", "we-dont-do-that-here.jpg"),
            text: $"Asking for prices outside of <#{config.GetValue<string>("ids:priceCheckChannel")}>?"
        );

        await ModifyOriginalResponseAsync(msg => msg.Content = "Image posted.");
    }
}
