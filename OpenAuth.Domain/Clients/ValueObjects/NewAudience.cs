using OpenAuth.Domain.Clients.Audiences.ValueObjects;

namespace OpenAuth.Domain.Clients.ValueObjects;

public sealed record NewAudience(AudienceName Name, ScopeCollection Scopes) : IEquatable<NewAudience>
{
    public NewAudience SetName(AudienceName name)
        => this with { Name = name };
    
    public NewAudience SetScopes(ScopeCollection scopes)
        => this with { Scopes = scopes };

    public bool Equals(NewAudience? other)
    {
        if (other is null)
            return false;
        
        return Name == other.Name && Scopes.Equals(other.Scopes);
    }
    
    public override int GetHashCode()
        => HashCode.Combine(Name, Scopes);
}