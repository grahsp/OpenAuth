using OpenAuth.Domain.Clients.Audiences;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class AudienceBuilder
{
    private AudienceName? _name;
    private DateTimeOffset? _createdAt;
    
    public AudienceBuilder WithName(string name)
    {
        _name = AudienceName.Create(name);
        return this;
    }

    public AudienceBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Audience Build()
    {
        var name = _name ?? AudienceName.Create("api");
        var createdAt = _createdAt ?? DateTimeOffset.UtcNow;
        
        return Audience.Create(name, createdAt);
    }
}