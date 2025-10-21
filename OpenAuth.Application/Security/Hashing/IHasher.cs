using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Application.Security.Hashing;

public interface IHasher
{
    string Hash(string plain);
    bool Verify(string plain, string encoded);
}