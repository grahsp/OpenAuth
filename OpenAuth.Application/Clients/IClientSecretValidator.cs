using OpenAuth.Domain.Clients;

namespace OpenAuth.Application.Clients;

public interface IClientSecretValidator
{
    bool Verify(string plain, Client client);
}