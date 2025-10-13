using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public class ClientCredentialsTokenIssuer : TokenIssuerBase<ClientCredentialsTokenRequest>
{
    public override GrantType GrantType => GrantType.ClientCredentials;
    
    private readonly ISecretQueryService _secretQueryService;
    
    public ClientCredentialsTokenIssuer(ISecretQueryService secretQueryService)
    {
        _secretQueryService = secretQueryService;
    }


    protected override async Task<TokenContext> IssueToken(ClientCredentialsTokenRequest request, CancellationToken ct = default)
    {
        var isValid = await _secretQueryService.ValidateSecretAsync(request.ClientId, request.ClientSecret, ct);
        if (!isValid)
            throw new UnauthorizedAccessException("Invalid client credentials.");

        return new TokenContext(
            request.ClientId,
            request.ClientId.ToString(),
            request.RequestedAudience,
            request.RequestedScopes
        );
    }
}