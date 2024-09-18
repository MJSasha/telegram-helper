namespace TelegramHelper.Utils;

public static class TagsExtensions
{
    private static readonly char[] Delimiters = [' ', '\n', '\r'];

    public static string RemoveTags(this string text)
    {
        var words = text.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Where(word => !word.StartsWith('#')));
    }

    public static IEnumerable<string> ExtractTags(this string text)
    {
        var tags = text.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.StartsWith('#'))
            .Distinct();
        return tags;
    }
}