using System.Text.RegularExpressions;

namespace TelegramHelper.Utils;

public static class TagsExtensions
{
    private static readonly char[] Delimiters = [' ', '\n', '\r'];

    public static string RemoveTags(this string text)
    {
        var pattern = @"\s*#\w+\s*";
        return Regex.Replace(text, pattern, " ").Trim();
    }

    public static IEnumerable<string> ExtractTags(this string text)
    {
        var tags = text.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.StartsWith('#'))
            .Distinct();
        return tags;
    }
}