using TelegramHelper.Domain.Entities;

namespace TelegramHelper.Utils;

public static class Extensions
{
    public static (string, string) GetCategoryButton(this Category category)
    {
        return (
            category.Name.TrimName(),
            $"category:{category.Id}"
        );
    }

    public static string TrimName(this string name) => name.Length > 20 ? $"{name[..17]}..." : name;
}