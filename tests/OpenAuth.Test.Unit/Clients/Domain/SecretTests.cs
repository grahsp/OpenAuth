using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Unit.Clients.Domain;

public class SecretTests
{
    private readonly FakeTimeProvider _time = new ();
    
    
    [Fact]
    public void Ctor_Sets_Hash_And_OptionalExpiry()
    {
        var hash = new SecretHash("v1$1$abc$xyz");
        var now = DateTime.UtcNow;
        var lifetime = TimeSpan.FromHours(1);

        var secret = new SecretBuilder()
            .WithHash(hash)
            .WithCreatedAt(now)
            .WithLifetime(lifetime)
            .Build();

        Assert.NotEqual(Guid.Empty, secret.Id.Value);
        Assert.Equal(hash, secret.Hash);
        Assert.Equal(now.Add(lifetime), secret.ExpiresAt);
        Assert.Null(secret.RevokedAt);
        
        // CreatedAt should be near now
        Assert.InRange(secret.CreatedAt, now.AddSeconds(-5), now.AddSeconds(5));
    }

    [Fact]
    public void IsActive_True_When_NoExpiryAndNotRevoked()
    {
        var secret = new SecretBuilder().Build();
        Assert.True(secret.IsActive(_time.GetUtcNow()));
    }

    [Fact]
    public void IsActive_False_When_Expired()
    {
        var lifetime = TimeSpan.FromDays(1);
        var secret = new SecretBuilder()
            .WithCreatedAt(_time.GetUtcNow())
            .WithLifetime(lifetime)
            .Build();
        
        _time.Advance(lifetime);
        Assert.False(secret.IsActive(_time.GetUtcNow()));
    }

    [Fact]
    public void IsActive_True_When_ExpiresInFuture()
    {
        var secret = new SecretBuilder().Build();
        Assert.True(secret.IsActive(_time.GetUtcNow()));
    }

    [Fact]
    public void Revoke_IsIdempotent_And_Disables()
    {
        var secret = new SecretBuilder().Build();
        Assert.True(secret.IsActive(_time.GetUtcNow()));

        secret.Revoke(_time.GetUtcNow());
        var first = secret.RevokedAt;
        Assert.False(secret.IsActive(_time.GetUtcNow()));
        Assert.NotNull(first);

        // Calling again does not change the timestamp
        secret.Revoke(_time.GetUtcNow());
        Assert.Equal(first, secret.RevokedAt);
    }
}