using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Test.Unit.Entities;

public class SigningKeyTests
{
    [Fact]
    public void CreateSymmetric_SetsExpectedProperties()
    {
        var expires = DateTime.UtcNow.AddHours(1);
        var key = new SigningKey(SigningAlgorithm.Hmac, "secret-value", expires);

        Assert.Equal(SigningAlgorithm.Hmac, key.Algorithm);
        Assert.Equal("secret-value", key.PrivateKey);
        Assert.NotEqual(default, key.KeyId);
        Assert.True((DateTime.UtcNow - key.CreatedAt).TotalSeconds < 5);
        Assert.Equal(expires, key.ExpiresAt);
    }

    [Fact]
    public void CreateAsymmetric_SetsExpectedProperties()
    {
        var expires = DateTime.UtcNow.AddHours(1);
        var key = new SigningKey(SigningAlgorithm.Rsa, "private-pem", expires);

        Assert.Equal(SigningAlgorithm.Rsa, key.Algorithm);
        Assert.Equal("private-pem", key.PrivateKey);
        Assert.NotEqual(default, key.KeyId);
        Assert.Equal(expires, key.ExpiresAt);
    }

    [Fact]
    public void IsActive_True_WhenNoExpiry()
    {
        var key = new SigningKey(SigningAlgorithm.Hmac, "secret");
        Assert.True(key.IsActive());
    }

    [Fact]
    public void IsActive_True_WhenExpiryInFuture()
    {
        var key = new SigningKey(SigningAlgorithm.Hmac, "secret", DateTime.UtcNow.AddMinutes(5));
        Assert.True(key.IsActive());
    }

    [Fact]
    public void IsActive_False_WhenExpired()
    {
        var key = new SigningKey(SigningAlgorithm.Hmac, "secret", DateTime.UtcNow.AddMinutes(-1));
        Assert.False(key.IsActive());
    } 
}