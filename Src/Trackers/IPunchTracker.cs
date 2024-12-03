namespace Kozma.net.Src.Trackers;

public interface IPunchTracker
{
    void SetPlayer(ulong id, string key);
    void AddEntry(ulong id, string key, string type, string grade);
    string GetData(ulong id, string key);
}
