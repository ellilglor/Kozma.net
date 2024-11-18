using System.Text.RegularExpressions;
using Kozma.net.Src.Helpers;

namespace Kozma.net.Helpers;

public partial class ContentHelper : IContentHelper
{
    private record TermFilter(string Before, string After, string? Exclude);

    private readonly List<TermFilter> filters = [new TermFilter("mixmaster", "overcharged mixmaster", "overcharged"),
        new TermFilter("totem", "somnambulists totem", "somnambulists"),
        new TermFilter("orbit gun", "orbitgun", null),
        new TermFilter("orbitgun", "celestial orbitgun", "celestial"),
        new TermFilter("daybreaker", "daybreaker band", "band"),
        new TermFilter("mixer", "overcharged mixmaster", null),
        new TermFilter("soaker", "spiral soaker", "spiral"),
        new TermFilter("blitz", "blitz needle", "needle"),
        new TermFilter("calad", "caladbolg", "bolg"),
        new TermFilter("ctr m", "ctr med", "ctr med"),
        new TermFilter("ctr h", "ctr high", "ctr high"),
        new TermFilter("asi m", "asi med", "asi med"),
        new TermFilter("asi h", "asi high", "asi high"),
        new TermFilter("ctr very high asi very high", "asi very high ctr very high", null),
        new TermFilter("lite gm", "asi high ctr very high", null),
        new TermFilter("lite gm ", "asi high ctr very high ", null),
        new TermFilter("gm lite", "asi high ctr very high", null),
        new TermFilter("gm lite ", "asi high ctr very high ", null),
        new TermFilter("gm", "asi very high ctr very high", null),
        new TermFilter("gm ", "asi very high ctr very high ", null),
        new TermFilter("medium", "med", null),
        new TermFilter("vhigh", "very high", null),
        new TermFilter("maximum", "max", null),
        new TermFilter("bk ", "black kat ", null),
        new TermFilter("bkc", "black kat cowl", null),
        new TermFilter("bkr", "black kat raiment", null),
        new TermFilter("bkm", "black kat mail", null),
        new TermFilter("ssb", "swiftstrike buckler", null),
        new TermFilter("btb", "barbarous thorn blade", null),
        new TermFilter("reciever", "receiver", null)];

    public string FilterContent(string content)
    {
        var filtered = content
            .ToLower()
            .Replace("vh", "very high");
            
        filtered = SpecialCharacters().Replace(filtered, string.Empty);

        foreach (var filter in filters)
        {
            if (filtered.Contains(filter.Before))
            {
                if (filter.Exclude != null && filtered.Contains(filter.Exclude))
                    continue;

                filtered = filtered.Replace(filter.Before, filter.After);
            }
        }

        return filtered;
    }

    [GeneratedRegex(@"['""’\+\[\]()\-{},|]")]
    private static partial Regex SpecialCharacters();
}
