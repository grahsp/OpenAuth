using OpenAuth.Application.Security.Secrets;
using OpenAuth.Domain.Clients;

namespace OpenAuth.Application.Clients;

public class ClientSecretValidator : IClientSecretValidator
{
    private readonly ISecretHasher _hasher;
    private readonly TimeProvider _time;

    public ClientSecretValidator(ISecretHasher hasher, TimeProvider time)
    {
        _hasher = hasher;
        _time = time;
    }
    
    
    public bool Verify(string plain, Client client)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(plain);
        if (string.IsNullOrWhiteSpace(plain))
            return false;
        
        return client.ActiveSecrets(_time.GetUtcNow()).Any(secret => _hasher.Verify(plain, secret.Hash));
    }
}