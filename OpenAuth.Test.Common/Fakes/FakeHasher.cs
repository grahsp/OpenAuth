using OpenAuth.Application.Security.Secrets;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Test.Common.Fakes;

public class FakeHasher : ISecretHasher
{
    private readonly string _expected;
    public FakeHasher(string expected) => _expected = expected;

    public SecretHash Hash(string plain) => new SecretHash("plain");

    public bool Verify(string plain, SecretHash encoded)
        => encoded.Value == plain && plain == _expected;
}