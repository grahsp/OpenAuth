using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Services;

public interface ISecretHasher
{
    SecretHash Hash(string plain);
    bool Verify(string plain, SecretHash encoded);
}