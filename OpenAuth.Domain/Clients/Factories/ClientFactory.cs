using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.Services;

namespace OpenAuth.Domain.Clients.Factories;

public class ClientFactory : IClientFactory
{
    private readonly ISecretHashProvider _hashProvider;
    private readonly TimeProvider _time;
    
    public ClientFactory(ISecretHashProvider hashProvider, TimeProvider time)
    {
        _hashProvider = hashProvider;
        _time = time;
    }
    
    
    [Obsolete]
    public Client Create(ClientName name)
        => Client.Create(name, _time.GetUtcNow());
    
    public Client Create(ClientConfiguration config, out string? plainSecret)
    {
        ArgumentNullException.ThrowIfNull(config);
        
        var now = _time.GetUtcNow();
        
        var client = Client.Create(config, now);
        plainSecret = null;
        
        if (config.ApplicationType.AllowsClientSecrets)
        {
            var result = _hashProvider.Create();
            plainSecret = result.PlainSecret;
            client.AddSecret(result.Hash, now);
        }
        
        client.ValidateClient();
        
        return client;
    }
}