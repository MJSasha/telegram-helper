using Microsoft.EntityFrameworkCore;

namespace TelegramHelper.Domain;

internal static class Extensions
{
    public static IQueryable<TEntity> IncludeMultiple<TEntity>(this IQueryable<TEntity> query, params string[] includes) where TEntity : class
    {
        if (includes == null) return query;
        return includes.Aggregate(query, (current, include) => current.Include(include));
    }

    public static IQueryable<TEntity> IncludeRecursive<TEntity>(this IQueryable<TEntity> query, string[] properties) where TEntity : class
    {
        if (properties == null) return query;
        return properties.Aggregate(query, (current, property) => typeof(TEntity).GetProperty(property) != null ? current.Include(property) : current);
    }
}