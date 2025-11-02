using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class SecuritySettingsTests
{
    [Fact]
    public void Ctor_WhenValidInput_Succeeds()
    {
        var lifetime = TimeSpan.FromMinutes(5);
        var settings = new SecuritySettingsBuilder()
            .WithTokenLifetime(lifetime)
            .Build();
        
        Assert.Empty(settings.Secrets);
        Assert.Equal(lifetime, settings.TokenLifetime);
    }

    [Fact]
    public void Ctor_WhenDuplicateSecretId_ThrowsException()
    {
        var builder = new SecretBuilder()
            .WithId();
        var secrets = new[] {builder.Build(), builder.Build()};

        Assert.Throws <ArgumentException>(()
            => new SecuritySettings(secrets, TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void Ctor_WhenInvalidTokenLifetime_ThrowsException()
    {
        var secret = new SecretBuilder().Build();
        
        Assert.Throws<ArgumentOutOfRangeException>(()
            => new SecuritySettings([secret], TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void AddSecret_WhenValidSecret_AddsSecret()
    {
        var settings = new SecuritySettingsBuilder().Build();
        var secret = new SecretBuilder().Build();
        
        var updated = settings.AddSecret(secret);

        Assert.Single(updated.Secrets);
    }
    
    [Fact]
    public void AddSecret_WhenDuplicateSecretId_ThrowsException()
    {
        var builder = new SecretBuilder()
            .WithId();
        
        var settings = new SecuritySettingsBuilder()
            .WithSecrets(builder.Build())
            .Build();

        Assert.Throws<ArgumentException>(()
            => settings.AddSecret(builder.Build()));
    }

    [Fact]
    public void SetTokenLifetime_WhenValidValue_UpdatesLifetime()
    {
        var settings = new SecuritySettingsBuilder().Build();
        var lifetime = TimeSpan.FromDays(1);
        
        var updated = settings.SetTokenLifetime(lifetime);
        
        Assert.Equal(lifetime, updated.TokenLifetime);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10000)]
    public void SetTokenLifetime_WhenInvalidValue_ThrowsException(int days)
    {
        var settings = new SecuritySettingsBuilder().Build();

        Assert.Throws<ArgumentOutOfRangeException>(()
            => settings.SetTokenLifetime(TimeSpan.FromDays(days)));
    }
}