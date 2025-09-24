using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Unit.Entities;

public class SigningKeyTests
{
    [Fact]
    public void CreateSymmetric_SetsExpectedProperties()
    {
        var expires = DateTime.UtcNow.AddHours(1);
        var key = new SigningKey(SigningAlgorithm.Hmac, new Key("secret-value"), DateTime.MinValue, expires);

        Assert.Equal(SigningAlgorithm.Hmac, key.Algorithm);
        Assert.Equal("secret-value", key.Key.Value);
        Assert.NotEqual(default, key.Id);
        Assert.True((DateTime.UtcNow - key.CreatedAt).TotalSeconds < 5);
        Assert.Equal(expires, key.ExpiresAt);
    }

    [Fact]
    public void CreateAsymmetric_SetsExpectedProperties()
    {
        var expires = DateTime.UtcNow.AddHours(1);
        var key = new SigningKey(SigningAlgorithm.Rsa, new Key("private-pem"), DateTime.MinValue, expires);

        Assert.Equal(SigningAlgorithm.Rsa, key.Algorithm);
        Assert.Equal(new Key("private-pem"), key.Key);
        Assert.NotEqual(default, key.Id);
        Assert.Equal(expires, key.ExpiresAt);
    }

    [Fact]
    public void IsActive_True_WhenExpiryInFuture()
    {
        var key = new SigningKey(SigningAlgorithm.Hmac, new Key("secret"), DateTime.MinValue, DateTime.MaxValue);
        Assert.True(key.IsActive(DateTime.MinValue));
    }

    [Fact]
    public void IsActive_False_WhenExpired()
    {
        var key = new SigningKey(SigningAlgorithm.Hmac, new Key("secret"), DateTime.MinValue, DateTime.MinValue);
        Assert.False(key.IsActive(DateTime.MaxValue));
    } 
}