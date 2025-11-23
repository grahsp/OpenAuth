using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public class ClientCredentialsTokenIssuer : TokenRequestProcessor<ClientCredentialsTokenCommand>
{
    public override GrantType GrantType => GrantType.ClientCredentials;
    
    private readonly ISecretQueryService _secretQueryService;
    
    public ClientCredentialsTokenIssuer(ISecretQueryService secretQueryService)
    {
        _secretQueryService = secretQueryService;
    }


    protected override async Task<TokenContext> ProcessAsync(ClientCredentialsTokenCommand command, CancellationToken ct = default)
    {
        if (!await _secretQueryService.ValidateSecretAsync(command.ClientId, command.ClientSecret, ct))
                throw new InvalidClientException("Invalid client credentials.");

        return new TokenContext(
            command.ClientId,
            command.ClientId.ToString(),
            command.RequestedAudience,
            command.RequestedScopes
        );
    }
}