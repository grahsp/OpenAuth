using OpenAuth.Domain.Entities;

namespace OpenAuth.Domain.Abstractions;

public interface IClientSecretValidator
{
    bool Verify(string plain, Client client);
}