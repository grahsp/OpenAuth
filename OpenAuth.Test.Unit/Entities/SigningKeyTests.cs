using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Test.Unit.Entities;

public class SigningKeyTests
{
    [Fact]
    public void CreateSymmetric_SetsExpectedProperties()
    {
        var expires = DateTime.UtcNow.AddHours(1);
        var key = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-value", expires);

        Assert.Equal(SigningAlgorithm.Hmac, key.Algorithm);
        Assert.Equal("secret-value", key.PrivateKey);
        Assert.Null(key.PublicKey); // Symmetric algorithms should not expose keys
        Assert.NotEqual(default, key.KeyId);
        Assert.True((DateTime.UtcNow - key.CreatedAt).TotalSeconds < 5);
        Assert.Equal(expires, key.ExpiresAt);
    }

    [Fact]
    public void CreateAsymmetric_SetsExpectedProperties()
    {
        var expires = DateTime.UtcNow.AddHours(1);
        var key = SigningKey.CreateAsymmetric(
            SigningAlgorithm.Rsa,
            "public-pem",
            "private-pem",
            expires);

        Assert.Equal(SigningAlgorithm.Rsa, key.Algorithm);
        Assert.Equal("private-pem", key.PrivateKey);
        Assert.Equal("public-pem", key.PublicKey);
        Assert.NotEqual(default, key.KeyId);
        Assert.Equal(expires, key.ExpiresAt);
    }

    [Fact]
    public void IsActive_True_WhenNoExpiry()
    {
        var key = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret");
        Assert.True(key.IsActive());
    }

    [Fact]
    public void IsActive_True_WhenExpiryInFuture()
    {
        var key = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret", DateTime.UtcNow.AddMinutes(5));
        Assert.True(key.IsActive());
    }

    [Fact]
    public void IsActive_False_WhenExpired()
    {
        var key = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret", DateTime.UtcNow.AddMinutes(-1));
        Assert.False(key.IsActive());
    } 
}