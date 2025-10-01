using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Security;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Security;

public class ClientSecretValidatorTests
{
    private readonly TimeProvider _time = new FakeTimeProvider();
    
    
    [Fact]
    public void Verify_ReturnsTrue_WhenPlainMatchesActiveSecret()
    {
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher("secret1");
        var validator = new ClientSecretValidator(hasher);

        var secret = new ClientSecret(hasher.Hash("secret1"));
        client.AddSecret(secret, _time.GetUtcNow());

        Assert.True(validator.Verify("secret1", client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenPlainDoesNotMatch()
    {
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher("secret1");
        var validator = new ClientSecretValidator(hasher);

        var secret = new ClientSecret(hasher.Hash("secret1"));
        client.AddSecret(secret, _time.GetUtcNow());

        Assert.False(validator.Verify("wrong", client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenSecretIsRevoked()
    {
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher("secret1");
        var validator = new ClientSecretValidator(hasher);

        var secret = new ClientSecret(hasher.Hash("secret1"));
        secret.Revoke();
        client.AddSecret(secret, _time.GetUtcNow());

        Assert.False(validator.Verify("secret1", client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenSecretIsExpired()
    {
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher("secret1");
        var validator = new ClientSecretValidator(hasher);

        var expired = new ClientSecret(hasher.Hash("secret1"), DateTime.UtcNow.AddSeconds(-10));
        client.AddSecret(expired, _time.GetUtcNow());

        Assert.False(validator.Verify("secret1", client));
    }

    [Fact]
    public void Verify_ReturnsTrue_WhenClientHasMultipleSecrets_AndOneMatches()
    {
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher("secret2");
        var validator = new ClientSecretValidator(hasher);

        client.AddSecret(new ClientSecret(hasher.Hash("secret1")), _time.GetUtcNow()); // won't match
        client.AddSecret(new ClientSecret(hasher.Hash("secret2")), _time.GetUtcNow()); // will match

        Assert.True(validator.Verify("secret2", client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenClientHasNoSecrets()
    {
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher("secret1");
        var validator = new ClientSecretValidator(hasher);

        Assert.False(validator.Verify("secret1", client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenPlainIsEmpty()
    {
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher("secret1");
        var validator = new ClientSecretValidator(hasher);

        client.AddSecret(new ClientSecret(hasher.Hash("secret1")), _time.GetUtcNow());

        Assert.False(validator.Verify("", client));
        Assert.False(validator.Verify("   ", client));
    }

    [Fact]
    public void Verify_Throws_WhenClientIsNull()
    {
        var hasher = new FakeHasher("secret1");
        var validator = new ClientSecretValidator(hasher);

        Assert.Throws<ArgumentNullException>(() =>
            validator.Verify("secret1", null!));
    }
}