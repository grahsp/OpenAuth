using OpenAuth.Domain.Abstractions;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Application.Security;

public class ClientSecretValidator : IClientSecretValidator
{
    private readonly ISecretHasher _hasher;
    
    public ClientSecretValidator(ISecretHasher hasher)
    {
        _hasher = hasher;
    }

    public bool Verify(string plain, Client client)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(plain);
        if (string.IsNullOrWhiteSpace(plain))
            return false;
        
        return client.ActiveSecrets().Any(secret => _hasher.Verify(plain, secret.Hash));
    }
}