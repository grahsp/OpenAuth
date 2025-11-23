namespace OpenAuth.Domain.Users.ValueObjects;

public sealed record UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}