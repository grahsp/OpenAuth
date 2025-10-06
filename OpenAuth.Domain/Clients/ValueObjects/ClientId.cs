namespace OpenAuth.Domain.Clients.ValueObjects;

public readonly record struct ClientId(Guid Value)
{
    public static ClientId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}