namespace Kozma.net.Models;

public class TermFilter(string before, string after, string? exclude)
{
    public string Before { get; set; } = before;
    public string After { get; set; } = after;
    public string? Exclude { get; set; } = exclude;
}
