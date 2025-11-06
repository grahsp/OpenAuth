using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class AudienceTests
{
    [Fact]
    public void Constructor_SetsNameAndScopes()
    {
        var name = new AudienceName("api");
        var scopes = ScopeCollection.Parse("read write");

        var audience = new Audience(name, scopes);

        Assert.Equal(name, audience.Name);
        Assert.Equal(scopes, audience.Scopes);
    }

    [Fact]
    public void Equality_BasedOnNameAndScopes()
    {
        var a1 = new Audience(new AudienceName("api"), ScopeCollection.Parse("read"));
        var a2 = new Audience(new AudienceName("api"), ScopeCollection.Parse("read"));
        var a3 = new Audience(new AudienceName("identity"), ScopeCollection.Parse("read"));

        Assert.Equal(a1, a2);
        Assert.NotEqual(a1, a3);
    }
}