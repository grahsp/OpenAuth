namespace OpenAuth.Domain.Clients.ValueObjects;

public sealed record Audience(AudienceName Name, ScopeCollection Scopes)
{
    public bool Equals(Audience? other)
    {
        if (other is null)
            return false;
        
        return Name == other.Name && Scopes.Equals(other.Scopes);
    }
    
    public override int GetHashCode()
        => HashCode.Combine(Name, Scopes);
}