using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Application.Security.Hashing;

public interface ISecretHasher
{
    SecretHash Hash(string plain);
    bool Verify(string plain, SecretHash encoded);
}