using System.Text.RegularExpressions;
using TelegramHelper.Definitions;
using TelegramHelper.Domain.Entities;

namespace TelegramHelper.Utils;

public static class Extensions
{
    public static (string, string) GetCategoryButton(this Category category)
    {
        return (
            category.Name,
            $"{CallbacksTags.ParentCategory}:{category.Id};"
        );
    }

    public static (string, string) GetNoteButton(this Note note)
    {
        return (
            note.Title,
            $"{CallbacksTags.Note}:{note.Id};"
        );
    }

    public static string? GetTagValue(this string? tagString, string tagName)
    {
        var pattern = $@"{tagName}:(.*?);";
        var match = Regex.Match(tagString, pattern);
        return match.Success ? match.Groups[1].Value : null;
    }
}