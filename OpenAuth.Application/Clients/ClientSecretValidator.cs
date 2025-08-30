using OpenAuth.Domain.Abstractions;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Application.Clients;

public class ClientSecretValidator : IClientSecretValidator
{
    public ClientSecretValidator(ISecretHasher hasher)
    {
        _hasher = hasher;
    }
    
    private readonly ISecretHasher _hasher;

    public bool Verify(string plain, Client client)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(plain);
        if (string.IsNullOrWhiteSpace(plain))
            return false;
        
        return client.ActiveSecrets().Any(secret => _hasher.Verify(plain, secret.Hash));
    }
}