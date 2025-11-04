using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class NewAudienceTests
{
    [Fact]
    public void Constructor_SetsNameAndScopes()
    {
        var name = new AudienceName("api");
        var scopes = ScopeCollection.Parse("read write");

        var audience = new NewAudience(name, scopes);

        Assert.Equal(name, audience.Name);
        Assert.Equal(scopes, audience.Scopes);
    }

    [Fact]
    public void SetName_ReturnsNewInstance_WithUpdatedName()
    {
        var audience = new NewAudience(new AudienceName("api"), ScopeCollection.Parse("read"));
        var updated = audience.SetName(new AudienceName("identity"));

        Assert.NotSame(audience, updated);
        Assert.Equal("identity", updated.Name.Value);
        Assert.Equal(audience.Scopes, updated.Scopes);
    }

    [Fact]
    public void SetScopes_ReturnsNewInstance_WithUpdatedScopes()
    {
        var audience = new NewAudience(new AudienceName("api"), ScopeCollection.Parse("read"));
        var updated = audience.SetScopes(ScopeCollection.Parse("read write"));

        Assert.NotSame(audience, updated);
        Assert.Equal("api", updated.Name.Value);
        Assert.Contains("write", updated.Scopes.Scopes);
    }

    [Fact]
    public void Immutability_IsPreserved_WhenUpdating()
    {
        var audience = new NewAudience(new AudienceName("api"), ScopeCollection.Parse("read"));
        var updated = audience.SetScopes(ScopeCollection.Parse("read write"));

        // Original should remain unchanged
        Assert.Single(audience.Scopes.Scopes);
        Assert.Equal(2, updated.Scopes.Scopes.Count);
    }

    [Fact]
    public void Equality_BasedOnNameAndScopes()
    {
        var a1 = new NewAudience(new AudienceName("api"), ScopeCollection.Parse("read"));
        var a2 = new NewAudience(new AudienceName("api"), ScopeCollection.Parse("read"));
        var a3 = new NewAudience(new AudienceName("identity"), ScopeCollection.Parse("read"));

        Assert.Equal(a1, a2);
        Assert.NotEqual(a1, a3);
    }
}