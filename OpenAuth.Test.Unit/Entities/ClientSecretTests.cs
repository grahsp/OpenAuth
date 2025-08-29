using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Unit.Entities;

public class ClientSecretTests
{
    [Fact]
    public void Ctor_Sets_Hash_And_OptionalExpiry()
    {
        var hash = new SecretHash("v1$1$abc$xyz");
        var now = DateTime.UtcNow;
        var exp = now.AddHours(1);

        var s = new ClientSecret(hash, exp);

        Assert.NotEqual(Guid.Empty, s.Id.Value);
        Assert.Equal(hash, s.Hash);
        Assert.Equal(exp, s.ExpiresAt);
        Assert.Null(s.RevokedAt);
        
        // CreatedAt should be near now
        Assert.InRange(s.CreatedAt, now.AddSeconds(-5), now.AddSeconds(5));
    }

    [Fact]
    public void IsActive_True_When_NoExpiryAndNotRevoked()
    {
        var s = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
        Assert.True(s.IsActive());
    }

    [Fact]
    public void IsActive_False_When_Expired()
    {
        var s = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddSeconds(-1));
        Assert.False(s.IsActive());
    }

    [Fact]
    public void IsActive_True_When_ExpiresInFuture()
    {
        var s = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddMinutes(5));
        Assert.True(s.IsActive());
    }

    [Fact]
    public void Revoke_IsIdempotent_And_Disables()
    {
        var s = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddHours(1));
        Assert.True(s.IsActive());

        s.Revoke();
        var first = s.RevokedAt;
        Assert.False(s.IsActive());
        Assert.NotNull(first);

        // Calling again does not change the timestamp
        s.Revoke();
        Assert.Equal(first, s.RevokedAt);
    }
}