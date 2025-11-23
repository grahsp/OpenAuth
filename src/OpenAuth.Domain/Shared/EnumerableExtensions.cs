namespace OpenAuth.Domain.Shared;

internal static class EnumerableExtensions
{
    public static IReadOnlyCollection<T> CreateDistinctCollection<T>(this IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .Where(u => u is not null)
            .Distinct()
            .ToArray();
    }

    public static IReadOnlyCollection<TKey> CreateDistinctCollection<TKey, TSource>(
        this IEnumerable<TKey> items,
        Func<TKey, TSource> selector)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .Where(u => u is not null)
            .DistinctBy(selector)
            .ToArray();
    }
}