using OpenAuth.Application.Security;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Security;

public class ClientSecretFactoryTests
{
    [Fact]
    public void Create_Returns_Plain_And_Populated_Secret()
    {
        var factory = new ClientSecretFactory(new FakeGenerator("PLAIN-123"), new EchoHasher());
        var expires = DateTime.UtcNow.AddDays(90);

        var (secret, plain) = factory.Create(expires);

        Assert.Equal((string?)"PLAIN-123", (string?)plain);
        Assert.Equal<DateTime?>(expires, secret.ExpiresAt);
        Assert.False(string.IsNullOrWhiteSpace(secret.Hash.Value));
        Assert.True((bool)secret.IsActive());
    }
}