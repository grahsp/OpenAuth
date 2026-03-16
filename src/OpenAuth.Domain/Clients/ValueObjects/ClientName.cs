using OpenAuth.Domain.Shared.ValueObjects;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record ClientName : Name
{
    public ClientName(string value) : base(value, 1, 100) { }

    public override string ToString() => Value;
}