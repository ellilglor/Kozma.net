namespace Kozma.net.Src.Trackers;

public interface IPunchTracker
{
    public void SetPlayer(ulong id, string key);
    public void AddEntry(ulong id, string key, string type, string grade);
    public string GetData(ulong id, string key);
}
