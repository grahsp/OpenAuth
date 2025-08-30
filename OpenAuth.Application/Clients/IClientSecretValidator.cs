using OpenAuth.Domain.Entities;

namespace OpenAuth.Application.Clients;

public interface IClientSecretValidator
{
    bool Verify(string plain, Client client);
}