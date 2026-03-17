using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.Services;

namespace OpenAuth.Application.Clients.Factories;

public class ClientFactory : IClientFactory
{
    private readonly ISecretHashProvider _hashProvider;
    private readonly TimeProvider _time;
    
    public ClientFactory(ISecretHashProvider hashProvider, TimeProvider time)
    {
        _hashProvider = hashProvider;
        _time = time;
    }
    
    public Client Create(CreateClientRequest request, out string? plainSecret)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var now = _time.GetUtcNow();
        
        plainSecret = null;
        var client = Client.Create(
            request.Name,
            request.ApplicationType,
            request.ApplicationType.DefaultGrantTypes,
            now);
        
        if (request.ApplicationType.AllowsClientSecrets)
        {
            var result = _hashProvider.Create();
            plainSecret = result.PlainTextSecret;
            client.AddSecret(result.Hash, now);
        }
        
        client.ValidateClient();
        
        return client;
    }
}