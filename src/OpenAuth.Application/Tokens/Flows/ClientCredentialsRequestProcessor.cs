using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public class ClientCredentialsRequestProcessor : TokenRequestProcessor<ClientCredentialsTokenCommand>
{
    public override GrantType GrantType => GrantType.ClientCredentials;
    
    private readonly ISecretQueryService _secretQueryService;
    
    public ClientCredentialsRequestProcessor(ISecretQueryService secretQueryService)
    {
        _secretQueryService = secretQueryService;
    }

    protected override async Task<TokenContext> ProcessAsync(ClientCredentialsTokenCommand command, ClientTokenData tokenData, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.ClientSecret))
            throw new InvalidRequestException("ClientSecret is required.");
        
        if (command.RequestedAudience is null)
            throw new InvalidRequestException("Audience is required.");

        if (command.RequestedScopes is null)
            throw new InvalidRequestException("Scopes is required.");
        
        var audience =  tokenData.AllowedAudiences.FirstOrDefault(a => a.Name == command.RequestedAudience)
            ?? throw new InvalidScopeException("Audience is not allowed.");
        
        if (!command.RequestedScopes.All(s => audience.Scopes.Contains(s)))
            throw new InvalidScopeException("One or more scopes are not allowed.");
        
        if (!await _secretQueryService.ValidateSecretAsync(command.ClientId, command.ClientSecret, ct))
            throw new InvalidClientException("Invalid client credentials.");

        return new TokenContext(
            command.ClientId,
            command.ClientId.ToString(),
            command.RequestedAudience.Value,
            command.RequestedScopes.Select(s => s.Value).ToArray()
        );
    }
}