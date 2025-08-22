using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Abstractions;

public interface ISecretHasher
{
    SecretHash Hash(string plain);
    bool Verify(string plain, SecretHash encoded);
}