using Kozma.net.Src.Enums;
using System.Globalization;
using System.Text;
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
        var filtered = SpecialCharsRegex().Replace(content, " ");

        foreach (var filter in Filters)
        {
            if (!filtered.Contains(filter.Before, StringComparison.OrdinalIgnoreCase)) continue;
            if (filter.Exclude != null && filtered.Contains(filter.Exclude, StringComparison.OrdinalIgnoreCase)) continue;

            filtered = filtered.Replace(filter.Before, filter.After, StringComparison.OrdinalIgnoreCase);
        }

        var result = filtered.RemoveExtraWhiteSpace().ToUpper(CultureInfo.InvariantCulture);
        return result;
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

        for (; i < len; i++)
        {
            var ch = src[i];
            if (char.IsWhiteSpace(ch))
            {
                if (skip) continue;
                src[index++] = ' ';
                skip = true;
            }
            else
            {
                skip = false;
                src[index++] = ch;
            }
        }

        return new string(src, 0, index);
    }

    public static string ToTitleCase(this string input)
    {
        var words = input.Split(' ');
        var final = new StringBuilder();

        foreach (var word in words)
        {
#pragma warning disable CA1308 // We want lowercase here
            final.Append(string.Concat(word[0].ToString().ToUpperInvariant(), word.ToLowerInvariant().AsSpan(1)) + ' ');
#pragma warning restore CA1308 // Normalize strings to uppercase
        }

        return final.ToString().TrimEnd();
    }
}
