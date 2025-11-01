using System.Diagnostics.CodeAnalysis;
using OpenAuth.Domain.Shared.Interfaces;
using OpenAuth.Domain.Shared.ValueObjects;

namespace OpenAuth.Domain.Clients.Audiences.ValueObjects;

public record AudienceName : Name, ICreate<string, AudienceName>
{
    public AudienceName(string value) : base(value, 1, 100) { }
    
    public static AudienceName Create(string value)
    {
        throw new NotImplementedException();
    }

    public static bool TryCreate(string value, [NotNullWhen(true)] out AudienceName? name)
    {
        try
        {
            name = new AudienceName(value);
            return true;
        }
        catch(ArgumentException _)
        {
            name = null;
            return false;
        }
    }
    
    public override string ToString() => Value;
}