namespace OpenAuth.Domain.SigningKeys.ValueObjects;

public readonly record struct SigningKeyId(Guid Value)
{
    public static SigningKeyId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}