using Kozma.net.Src.Enums;

namespace Kozma.net.Src.Trackers;

public interface IUnboxTracker
{
    void SetPlayer(ulong id, Box key);
    void AddEntry(ulong id, Box key, string value);
    string GetData(ulong id, Box key);
    int GetItemCount(ulong id, Box key);
}
