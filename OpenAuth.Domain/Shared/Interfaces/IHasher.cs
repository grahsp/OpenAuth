namespace OpenAuth.Domain.Shared.Interfaces;

public interface IHasher
{
    string Hash(string plain);
    bool Verify(string plain, string encoded);
}