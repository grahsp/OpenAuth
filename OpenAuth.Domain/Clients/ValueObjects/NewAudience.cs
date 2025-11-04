using OpenAuth.Domain.Clients.Audiences.ValueObjects;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record NewAudience(AudienceName Name, ScopeCollection Scopes)
{
    public NewAudience SetName(AudienceName name)
        => this with { Name = name };
    
    public NewAudience SetScopes(ScopeCollection scopes)
        => this with { Scopes = scopes };
}