using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Api.Identity.Token;

public static class TokenRequestMappings
{
    public static TokenCommand ToCommand(this TokenRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.GrantType))
            throw new InvalidOperationException("grant_type is required.");
        
        return dto.GrantType switch
        {
            GrantTypes.AuthorizationCode => MapAuthorizationCode(dto),
            GrantTypes.ClientCredentials => MapClientCredentials(dto),
            _ => throw new NotSupportedException("unsupported grant_type.")
        };
    }

    private static AuthorizationCodeTokenCommand MapAuthorizationCode(TokenRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new InvalidOperationException("code is required.");

        if (string.IsNullOrWhiteSpace(dto.RedirectUri))
            throw new InvalidOperationException("redirect_uri is required.");
        
        if (!ClientId.TryCreate(dto.ClientId, out var clientId))
            throw new InvalidOperationException("Invalid client_id parameter.");
        
        if (!RedirectUri.TryCreate(dto.RedirectUri, out var redirectUri))
            throw new InvalidOperationException("Invalid redirect_uri parameter.");

        if (!AudienceName.TryCreate(dto.Audience, out var audience) && dto.Audience is not null)
            throw new InvalidOperationException("Invalid audience parameter.");

        if (!ScopeCollection.TryParse(dto.Scope, out var scope) && dto.Scope is not null)
            throw new InvalidOperationException("Invalid scope parameter.");

        return AuthorizationCodeTokenCommand.Create(
            dto.Code,
            clientId,
            redirectUri,
            audience,
            scope,
            dto.CodeVerifier,
            dto.ClientSecret
        );
    }

    private static ClientCredentialsTokenCommand MapClientCredentials(TokenRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClientSecret))
            throw new InvalidOperationException("client_secret is required.");
        
        if (!ClientId.TryCreate(dto.ClientId, out var clientId))
            throw new InvalidOperationException("Invalid client_id parameter.");
        
        if (!AudienceName.TryCreate(dto.Audience, out var audience) && dto.Audience is not null)
            throw new InvalidOperationException("Invalid audience parameter.");

        if (!ScopeCollection.TryParse(dto.Scope, out var scope) && dto.Scope is not null)
            throw new InvalidOperationException("Invalid scope parameter.");

        return ClientCredentialsTokenCommand.Create(
            clientId,
            audience,
            scope,
            dto.ClientSecret
        );
    }
}