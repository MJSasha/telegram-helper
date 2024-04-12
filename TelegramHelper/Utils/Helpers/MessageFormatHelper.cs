using TelegramHelper.Domain.Entities;

namespace TelegramHelper.Utils.Helpers;

internal static class MessageFormatHelper
{
    public static string GetCategoryHierarchy(Category category)
    {
        if (category == null) return string.Empty;

        var categoryHierarchy = new List<string>();
        BuildCategoryHierarchy(category, categoryHierarchy);
        categoryHierarchy.Reverse();

        return string.Join(" 👉 ", categoryHierarchy);
    }

    private static void BuildCategoryHierarchy(Category category, List<string> hierarchy)
    {
        if (category == null) return;

        hierarchy.Add(category.Name);
        BuildCategoryHierarchy(category.ParentCategory, hierarchy);
    }
}