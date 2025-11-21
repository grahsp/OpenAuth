using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public class ClientCredentialsTokenIssuer : TokenIssuerBase<ClientCredentialsTokenCommand>
{
    public override GrantType GrantType => GrantType.ClientCredentials;
    
    private readonly ISecretQueryService _secretQueryService;
    
    public ClientCredentialsTokenIssuer(ISecretQueryService secretQueryService)
    {
        _secretQueryService = secretQueryService;
    }


    protected override async Task<TokenContext> IssueToken(ClientCredentialsTokenCommand command, CancellationToken ct = default)
    {
        var isValid = await _secretQueryService.ValidateSecretAsync(command.ClientId, command.ClientSecret, ct);
        if (!isValid)
            throw new UnauthorizedAccessException("Invalid client credentials.");

        return new TokenContext(
            command.ClientId,
            command.ClientId.ToString(),
            command.RequestedAudience,
            command.RequestedScopes
        );
    }
}