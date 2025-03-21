﻿using Discord;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Handlers;
using Microsoft.Extensions.Caching.Memory;

namespace Kozma.net.Src.Helpers;

class DiscordPaginator(IBot bot, IMemoryCache cache, IEmbedHandler embedHandler) : IDiscordPaginator
{
    private static readonly MemoryCacheEntryOptions _cacheOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };

    public Embed GetPage(string pagesKey, string userKey, string action)
    {
        if (!cache.TryGetValue(pagesKey, out List<Embed>? pages) || pages is null) return embedHandler.GetAndBuildEmbed("Pages don't exist anymore, rerun the command.");
        if (!cache.TryGetValue(userKey, out int currentPage)) currentPage = 0;

        switch (action)
        {
            case ComponentIds.First: currentPage = 0; break;
            case ComponentIds.Previous when currentPage > 0: currentPage--; break;
            case ComponentIds.Next when currentPage < pages.Count - 1: currentPage++; break;
            case ComponentIds.Last: currentPage = pages.Count - 1; break;
            default: currentPage = 0; break;
        }

        cache.Set(userKey, currentPage, _cacheOptions);
        return pages[currentPage];
    }

    public MessageComponent GetComponents(string pagesKey, string userKey, string baseId)
    {
        if (!cache.TryGetValue(userKey, out int page)) page = 0;
        if (!cache.TryGetValue(pagesKey, out List<Embed>? pages) || pages is null) pages = [];

        return new ComponentBuilder()
            .WithButton(label: Emotes.First, customId: baseId + ComponentIds.First, style: ButtonStyle.Primary, disabled: page == 0)
            .WithButton(label: Emotes.Previous, customId: baseId + ComponentIds.Previous, style: ButtonStyle.Primary, disabled: page == 0)
            .WithButton(label: Emotes.Next, customId: baseId + ComponentIds.Next, style: ButtonStyle.Primary, disabled: page >= pages.Count - 1)
            .WithButton(label: Emotes.Last, customId: baseId + ComponentIds.Last, style: ButtonStyle.Primary, disabled: page >= pages.Count - 1)
            .Build();
    }

    public void AddPageCounterAndSaveToCache(IList<EmbedBuilder> pages, string pagesKey, bool addTitle = false)
    {
        var finalPages = new List<Embed>();

        for (int i = 0; i < pages.Count; i++)
        {
            if (addTitle) pages[i].Title = $"{pages[i].Title} - {i + 1}/{pages.Count}";
            pages[i].Footer = new EmbedFooterBuilder()
                .WithText($"{i + 1}/{pages.Count}")
                .WithIconUrl(bot.Client.CurrentUser.GetDisplayAvatarUrl());

            finalPages.Add(pages[i].Build());
        }

        cache.Set(pagesKey, finalPages, _cacheOptions);
    }
}
