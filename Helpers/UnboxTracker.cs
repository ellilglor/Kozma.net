using Kozma.net.Enums;
using System.Text;

namespace Kozma.net.Helpers;

public class UnboxTracker : IGameTracker
{
    private record Item(string Name, int Count);
    private readonly Dictionary<ulong, Dictionary<Box, List<Item>>> _items = [];

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
            _items[id][key].Add(new Item(value, 1));
        }
        else
        {
            _items[id][key][_items[id][key].IndexOf(item)] = item with { Count = item.Count + 1 };
        }
    }

    public string GetData(ulong id, Box key)
    {
        CheckIfIdIsPresent(id);
        
        if (_items[id][key] is null || _items[id][key].Count == 0)
        {
            return "The bot has restarted and this data is lost!";
        }

        var data = new StringBuilder();
        var items = _items[id][key].OrderByDescending(i => i.Count);

        foreach (var item in items)
        {
            if (data.Length + item.Name.Length >= (int)DiscordCharLimit.EmbedDesc - 50)
            {
                data.AppendLine("**I have reached the character limit!**");
                break;
            }

            data.AppendLine($"{item.Name} : {item.Count}");
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
