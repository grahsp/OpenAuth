namespace OpenAuth.Domain.Shared;

internal static class DomainRules
{
    public static void EnsureCollectionNotEmpty<T>(
        IEnumerable<T> items,
        string? message = "Collection must not be empty.")
    {
        ArgumentNullException.ThrowIfNull(items);
        
        if (!items.Any())
            throw new InvalidOperationException(message);
    }
    

}