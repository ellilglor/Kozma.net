using Kozma.net.Src.Enums;

namespace Kozma.net.Src.Trackers;

public interface IUnboxTracker
{
    public void SetPlayer(ulong id, Box key);
    public void AddEntry(ulong id, Box key, string value);
    public string GetData(ulong id, Box key);
    public int GetItemCount(ulong id, Box key);
}
