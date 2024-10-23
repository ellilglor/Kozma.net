namespace Kozma.net.Models;

public class Channel(string name, int count, string time)
{
    public string Name { get; set; } = name;
    public int Count { get; set; } = count;
    public string Time { get; set; } = time;
}
