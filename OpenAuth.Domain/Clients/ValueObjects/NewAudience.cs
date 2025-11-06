using OpenAuth.Domain.Clients.Audiences.ValueObjects;

namespace OpenAuth.Domain.Clients.ValueObjects;

public sealed record NewAudience(AudienceName Name, ScopeCollection Scopes)
{
    public bool Equals(NewAudience? other)
    {
        if (other is null)
            return false;
        
        return Name == other.Name && Scopes.Equals(other.Scopes);
    }
    
    public override int GetHashCode()
        => HashCode.Combine(Name, Scopes);
}