using Kozma.net.Src.Enums;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kozma.net.Src.Extensions;
public static partial class StringExtensions
{
    private sealed record TermFilter(string Before, string After, string? Exclude);

    private static readonly List<TermFilter> Filters = [new TermFilter("mixmaster", "overcharged mixmaster", "overcharged"),
        new TermFilter("totem", "somnambulists totem", "somnambulists"),
        new TermFilter("orbit gun", "orbitgun", null),
        new TermFilter("orbitgun", "celestial orbitgun", "celestial"),
        new TermFilter("daybreaker", "daybreaker band", "band"),
        new TermFilter("mixer", "overcharged mixmaster", null),
        new TermFilter("soaker", "spiral soaker", "spiral"),
        new TermFilter("blitz", "blitz needle", "needle"),
        new TermFilter("calad", "caladbolg", "bolg"),
        new TermFilter("vh", "very high", null),
        new TermFilter("ctr m", "ctr med", "ctr med"),
        new TermFilter("ctr h", "ctr high", "ctr high"),
        new TermFilter("asi m", "asi med", "asi med"),
        new TermFilter("asi h", "asi high", "asi high"),
        new TermFilter("ctr very high asi very high", "asi very high ctr very high", null),
        new TermFilter("lite gm", "asi high ctr very high", null),
        new TermFilter("lite gm ", "asi high ctr very high ", null),
        new TermFilter("gm lite", "asi high ctr very high", null),
        new TermFilter("gm lite ", "asi high ctr very high ", null),
        new TermFilter(" gm", "asi very high ctr very high", null),
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

    public static string CleanUp(this string content)
    {
        var filtered = SpecialCharsRegex().Replace(content, string.Empty);

        foreach (var filter in Filters)
        {
            if (!filtered.Contains(filter.Before, StringComparison.OrdinalIgnoreCase)) continue;
            if (filter.Exclude != null && filtered.Contains(filter.Exclude, StringComparison.OrdinalIgnoreCase)) continue;

            filtered = filtered.Replace(filter.Before, filter.After, StringComparison.OrdinalIgnoreCase);
        }

        return filtered.RemoveExtraWhiteSpace().ToUpper(CultureInfo.InvariantCulture);
    }

    [GeneratedRegex(@"['""’\+\[\]()\-{},|]")]
    private static partial Regex SpecialCharsRegex();

    private static readonly Dictionary<string, PunchOption> PunchOptionMapping = new()
    {
        { "Brandish", PunchOption.Brandish },
        { "Overcharged Mixmaster", PunchOption.Mixmaster },
        { "Blast Bomb", PunchOption.Bomb },
        { "Swiftstrike Buckler", PunchOption.Shield },
        { "Black Kat Cowl", PunchOption.Helmet }
    };

    public static PunchOption ConvertToPunchOption(this string item) =>
        PunchOptionMapping.TryGetValue(item, out var data) ? data : throw new InvalidOperationException($"{item} is not a valid option");

    // https://stackoverflow.com/questions/6442421/c-sharp-fastest-way-to-remove-extra-white-spaces
    public static string RemoveExtraWhiteSpace(this string input)
    {
        int len = input.Length,
            index = 0,
            i = 0;
        var src = input.ToCharArray();
        bool skip = false;
        char ch;
        for (; i < len; i++)
        {
            ch = src[i];
            switch (ch)
            {
                case '\u0020':
                case '\u00A0':
                case '\u1680':
                case '\u2000':
                case '\u2001':
                case '\u2002':
                case '\u2003':
                case '\u2004':
                case '\u2005':
                case '\u2006':
                case '\u2007':
                case '\u2008':
                case '\u2009':
                case '\u200A':
                case '\u202F':
                case '\u205F':
                case '\u3000':
                case '\u2028':
                case '\u2029':
                case '\u0009':
                case '\u000A':
                case '\u000B':
                case '\u000C':
                case '\u000D':
                case '\u0085':
                    if (skip) continue;
                    src[index++] = ch;
                    skip = true;
                    continue;
                default:
                    skip = false;
                    src[index++] = ch;
                    continue;
            }
        }

        return new string(src, 0, index);
    }
}
