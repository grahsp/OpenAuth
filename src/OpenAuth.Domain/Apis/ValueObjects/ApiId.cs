namespace OpenAuth.Domain.Apis.ValueObjects;

public sealed record ApiId(Guid Value)
{
    public static ApiId New() => new(Guid.NewGuid());
}