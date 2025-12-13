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
    
    
    [Obsolete]
    public Client Create(ClientName name)
        => Client.Create(name, _time.GetUtcNow());
    
    public Client Create(CreateClientCommand command, out string? plainSecret)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var now = _time.GetUtcNow();
        
        plainSecret = null;
        var client = Client.Create(
            command.Name,
            command.ApplicationType,
            command.Permissions,
            command.ApplicationType.DefaultGrantTypes,
            command.RedirectUris,
            now);
        
        if (command.ApplicationType.AllowsClientSecrets)
        {
            var result = _hashProvider.Create();
            plainSecret = result.PlainSecret;
            client.AddSecret(result.Hash, now);
        }
        
        client.ValidateClient();
        
        return client;
    }
}