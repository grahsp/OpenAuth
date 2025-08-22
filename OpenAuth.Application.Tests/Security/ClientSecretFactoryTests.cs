using OpenAuth.Application.Security;
using OpenAuth.Application.Tests.Stubs;

namespace OpenAuth.Application.Tests.Security;

public class ClientSecretFactoryTests
{
    [Fact]
    public void Create_Returns_Plain_And_Populated_Secret()
    {
        var factory = new ClientSecretFactory(new FakeGenerator("PLAIN-123"), new EchoHasher());
        var expires = DateTime.UtcNow.AddDays(90);

        var (secret, plain) = factory.Create(expires);

        Assert.Equal("PLAIN-123", plain);
        Assert.Equal(expires, secret.ExpiresAt);
        Assert.False(string.IsNullOrWhiteSpace(secret.Hash.Value));
        Assert.True(secret.IsActive());
    }
}