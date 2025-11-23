namespace OpenAuth.Domain.Services;

public interface IHasher
{
    string Hash(string plain);
    bool Verify(string plain, string encoded);
}