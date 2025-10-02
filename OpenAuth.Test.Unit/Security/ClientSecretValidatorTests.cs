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
        
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);

        var client = new ClientBuilder()
            .WithSecret(hash)
            .Build();

        Assert.True(validator.Verify(hash, client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenPlainDoesNotMatch()
    {
        var hasher = new FakeHasher("secret");
        var validator = new ClientSecretValidator(hasher, _time);

        var client = new ClientBuilder()
            .WithSecret("secret")
            .Build();

        Assert.False(validator.Verify("wrong", client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenSecretIsRevoked()
    {
        const string hash = "secret";
        
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);

        var client = new ClientBuilder()
            .WithSecret(hash)
            .Build();
        
        client.Secrets.First().Revoke(_time.GetUtcNow());

        Assert.False(validator.Verify(hash, client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenSecretIsExpired()
    {
        const string hash = "secret";
        
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);

        var client = new ClientBuilder()
            .WithSecret(hash)
            .CreatedAt(_time.GetUtcNow())
            .Build();
        
        _time.Advance(TimeSpan.FromDays(365));
        
        Assert.False(validator.Verify(hash, client));
    }

    [Fact]
    public void Verify_ReturnsTrue_WhenClientHasMultipleSecrets_AndOneMatches()
    {
        const string matchingHash = "matching-secret";
        const string nonMatchingHash = "non-matching-secret";
        
        var hasher = new FakeHasher(matchingHash);
        var validator = new ClientSecretValidator(hasher, _time);

        var client = new ClientBuilder()
            .WithSecret(matchingHash)
            .WithSecret(nonMatchingHash)
            .Build();

        Assert.True(validator.Verify(matchingHash, client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenClientHasNoSecrets()
    {
        var hasher = new FakeHasher("secret");
        var validator = new ClientSecretValidator(hasher, _time);
        
        var client = new ClientBuilder()
            .Build();

        Assert.False(validator.Verify("wrong", client));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenPlainIsEmpty()
    {
        const string hash = "secret";
        
        var hasher = new FakeHasher(hash);
        var validator = new ClientSecretValidator(hasher, _time);

        var client = new ClientBuilder()
            .WithSecret(hash)
            .Build();

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