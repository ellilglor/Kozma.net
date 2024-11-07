﻿using Kozma.net.Enums;
using Kozma.net.Models;
using System.Text;

namespace Kozma.net.Trackers;

public class UnboxTracker : IUnboxTracker
{
    private readonly Dictionary<ulong, Dictionary<Box, List<TrackerItem>>> _items = [];

    public void SetPlayer(ulong id, Box key)
    {
        CheckIfIdIsPresent(id);
        _items[id][key] = [];
    }

    public void AddEntry(ulong id, Box key, string value)
    {
        CheckIfIdIsPresent(id);
        var item = _items[id][key].Find(i => i.Name == value);

        if (item is null)
        {
            _items[id][key].Add(new TrackerItem(value, 1));
        }
        else
        {
            _items[id][key][_items[id][key].IndexOf(item)] = item with { Count = item.Count + 1 };
        }
    }

    public string GetData(ulong id, Box key)
    {
        CheckIfIdIsPresent(id);

        if (!_items[id].TryGetValue(key, out List<TrackerItem>? unboxed) || unboxed.Count == 0)
        {
            return "The bot has restarted and this data is lost!";
        }

        var data = new StringBuilder();
        var items = unboxed.OrderByDescending(i => i.Count);

        foreach (var item in items)
        {
            if (data.Length + item.Name.Length >= (int)DiscordCharLimit.EmbedDesc - 50)
            {
                data.AppendLine("**I have reached the character limit!**");
                break;
            }

            data.AppendLine($"{item.Name}: {item.Count}");
        }

        return data.ToString();
    }

    public int GetItemCount(ulong id, Box key)
    {
        CheckIfIdIsPresent(id);
        return _items[id][key].Count;
    }

    private void CheckIfIdIsPresent(ulong id)
    {
        if (!_items.ContainsKey(id)) _items[id] = [];
    }
}