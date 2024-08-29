using Discord;
using System.Text.RegularExpressions;

namespace Kozma.net.Helpers;

public class ContentHelper : IContentHelper
{
    public string FilterContent(string content)
    {
        var filtered = content
            .ToLower()
            .Replace("vh", "very high");

        filtered = Regex.Replace(filtered, @"['""’\+\[\]()\-{},|]", " ");

        var pattern = "/['\"\\+\\[\\]()\\-{},]/g";
        item = Regex.Replace(item, pattern, string.Empty);


        return filtered;
    }
}
