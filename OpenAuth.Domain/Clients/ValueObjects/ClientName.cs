using System.Diagnostics.CodeAnalysis;
using OpenAuth.Domain.Shared.Interfaces;
using OpenAuth.Domain.Shared.ValueObjects;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record ClientName : Name, ICreate<string, ClientName>
{
    public ClientName(string value) : base(value, 1, 100) { }

    public static ClientName Create(string value)
        => new ClientName(value);

    public static bool TryCreate(string value, [NotNullWhen(true)] out ClientName? name)
    {
        try
        {
            name = new ClientName(value);
            return true;
        }
        catch (ArgumentException _)
        {
            name = null;
            return false;
        }
    }

    public override string ToString()
        => Value;
}