using OpenAuth.Domain.Services;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Security;

namespace OpenAuth.Infrastructure.Tests.Security;

public class ClientSecretFactoryTests
{
    private sealed class FakeGenerator : ISecretGenerator
    {
        private readonly string _value;
        public FakeGenerator(string v) => _value = v;

        public string Generate() => Generate(32);
        public string Generate(int byteLength) => _value;
    }

    private sealed class EchoHasher : ISecretHasher
    {
        public SecretHash Hash(string plain) => new SecretHash("v1$echo$salt$" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plain)));
        public bool Verify(string input, SecretHash encoded) => encoded.Value.EndsWith(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input)), StringComparison.Ordinal);
    }

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

    [Fact]
    public void Create_DifferentCalls_ProduceDifferentSecrets_WithRealDeps()
    {
        // Smoke test with real generator/hasher
        var factory = new ClientSecretFactory(new SecretGenerator(), new Pbkdf2Hasher(60_000));

        var (_, p1) = factory.Create();
        var (_, p2) = factory.Create();

        Assert.NotEqual(p1, p2);
    }
}