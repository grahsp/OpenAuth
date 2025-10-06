using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Application.Security.Secrets;

public interface ISecretHasher
{
    SecretHash Hash(string plain);
    bool Verify(string plain, SecretHash encoded);
}