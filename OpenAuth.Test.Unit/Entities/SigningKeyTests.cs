using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Entities;

public class SigningKeyTests
{
    private readonly TimeProvider _time = new FakeTimeProvider();
    
    
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var createdAt = _time.GetUtcNow();
        var expiresAt = createdAt.AddHours(1);
        var key = new Key("private-key");
        var signingKey = TestSigningKey.CreateRsaSigningKey(createdAt, expiresAt, key);
        
        Assert.NotEqual(default, signingKey.Id);
        Assert.Equal(SigningAlgorithm.RS256, signingKey.KeyMaterial.Alg);
        Assert.Equal(key, signingKey.KeyMaterial.Key);
        Assert.Equal(createdAt, signingKey.CreatedAt);
        Assert.Equal(expiresAt, signingKey.ExpiresAt);
        Assert.Null(signingKey.RevokedAt);
    }

    [Fact]
    public void IsActive_True_WhenNotExpiredAndNotRevoked()
    {
        var now = _time.GetUtcNow();
        var signingKey = TestSigningKey.CreateRsaSigningKey(_time);
        Assert.True(signingKey.IsActive(now));
    }

    [Fact]
    public void IsActive_False_WhenExpired()
    {
        var now = _time.GetUtcNow();
        var signingKey = TestSigningKey.CreateRsaSigningKey(now, now.AddMinutes(-1));
        
        Assert.False(signingKey.IsActive(now));
    } 
    
    [Fact]
    public void IsActive_False_WhenRevoked()
    {
        var now = _time.GetUtcNow();
        var signingKey = TestSigningKey.CreateRsaSigningKey(now, now.AddMinutes(5));
        signingKey.Revoke(now);
        
        Assert.False(signingKey.IsActive(now));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    public void HasExpired_ReturnsExpectedValue(int offsetMinutes, bool expected)
    {
        var now = _time.GetUtcNow();
        var expiresAt = now.AddMinutes(offsetMinutes);
        var signingKey = TestSigningKey.CreateRsaSigningKey(now, expiresAt);
        
        Assert.Equal(expected, signingKey.HasExpired(now));
    }
    
    [Fact]
    public void IsRevoked_FalseInitially()
    {
        var now = _time.GetUtcNow();
        var signingKey = TestSigningKey.CreateRsaSigningKey(now, now.AddMinutes(5));

        Assert.True(signingKey.IsActive(now));
    }
    
    [Fact]
    public void IsRevoked_TrueAfterRevocation()
    {
        var now = _time.GetUtcNow();
        var signingKey = TestSigningKey.CreateRsaSigningKey(now, now.AddMinutes(5));
        signingKey.Revoke(now);
        
        Assert.True(signingKey.IsRevoked());
    }

    [Fact]
    public void Revoke_SetsRevokedAt_AndReturnsTrue()
    {
        var now = _time.GetUtcNow();
        var signingKey = TestSigningKey.CreateRsaSigningKey(now, now.AddMinutes(5));
        
        var result = signingKey.Revoke(now);
        
        Assert.True(result);
        Assert.Equal(now, signingKey.RevokedAt);
    }

    [Fact]
    public void Revoke_ReturnsFalse_WhenAlreadyRevoked_AndDoesNotUpdateRevokedAt()
    {
        var now = _time.GetUtcNow();
        var signingKey = TestSigningKey.CreateRsaSigningKey(now, now.AddMinutes(5));
        signingKey.Revoke(now);

        var later = now.AddHours(1);
        var result = signingKey.Revoke(later);
        
        Assert.False(result);
        Assert.Equal(now, signingKey.RevokedAt);   
    }
}