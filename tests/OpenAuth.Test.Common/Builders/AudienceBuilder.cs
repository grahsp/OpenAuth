using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class AudienceBuilder
{
    private AudienceName? _name;
    private ScopeCollection? _scopes;
    
    public AudienceBuilder WithName(string name)
    {
        _name = AudienceName.Create(name);
        return this;
    }

    public AudienceBuilder WithScopes(string scopes)
    {
        _scopes = ScopeCollection.Parse(scopes);
        return this;
    }

    public Audience Build()
    {
        var name = _name ?? AudienceName.Create("api");
        var scopes = _scopes ?? ScopeCollection.Parse("read write");
        
        return new Audience(name, scopes);
    }
}