using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Clients;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Security;

public class ClientSecretValidatorTests
{
    private readonly FakeTimeProvider _time = new();
    
    
    [Fact]
    public void Verify_ReturnsTrue_WhenPlainMatchesActiveSecret()
    {
        const string hash = "secret";
        var client = new ClientBuilder().Build();
        
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);

        var secret = new ClientSecretBuilder()
            .WithHash(hash)
            .Build();
        client.AddSecret(secret, _time.GetUtcNow());

        Assert.True(validator.Verify(hash, client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenPlainDoesNotMatch()
    {
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher("secret");
        var validator = new ClientSecretValidator(hasher, _time);

        var secret = new ClientSecretBuilder().Build();
        client.AddSecret(secret, _time.GetUtcNow());

        Assert.False(validator.Verify("wrong", client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenSecretIsRevoked()
    {
        const string hash = "secret";
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);

        var secret = new ClientSecretBuilder().Build();
        secret.Revoke(_time.GetUtcNow());
        client.AddSecret(secret, _time.GetUtcNow());

        Assert.False(validator.Verify(hash, client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenSecretIsExpired()
    {
        const string hash = "secret";
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);

        var expired = new ClientSecretBuilder()
            .WithLifetime(TimeSpan.FromHours(1))
            .Build();
        
        client.AddSecret(expired, _time.GetUtcNow());
        Assert.False(validator.Verify(hash, client));
    }

    [Fact]
    public void Verify_ReturnsTrue_WhenClientHasMultipleSecrets_AndOneMatches()
    {
        var client = new ClientBuilder().Build();
        const string hash = "matching-secret";
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);

        client.AddSecret(new ClientSecretBuilder()
            .WithHash("no-match")
            .Build(), _time.GetUtcNow()); // won't match
        
        client.AddSecret(new ClientSecretBuilder()
            .WithHash(hash)
            .Build(), _time.GetUtcNow()); // will match

        Assert.True(validator.Verify(hash, client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenClientHasNoSecrets()
    {
        const string hash = "secret";
        var client = new ClientBuilder().Build();
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);

        Assert.False(validator.Verify(hash, client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenPlainIsEmpty()
    {
        var client = new ClientBuilder().Build();
        
        const string hash = "secret";
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);
        
        var secret = new ClientSecretBuilder()
            .WithHash(hash)
            .Build();

        client.AddSecret(secret, _time.GetUtcNow());

        Assert.False(validator.Verify("", client));
        Assert.False(validator.Verify("   ", client));
    }

    [Fact]
    public void Verify_Throws_WhenClientIsNull()
    {
        const string hash = "secret";
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);

        Assert.Throws<ArgumentNullException>(() =>
            validator.Verify(hash, null!));
    }
}