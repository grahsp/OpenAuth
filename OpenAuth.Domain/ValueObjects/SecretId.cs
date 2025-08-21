namespace OpenAuth.Domain.ValueObjects;

public readonly record struct SecretId(Guid Value)
{
    public static SecretId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}