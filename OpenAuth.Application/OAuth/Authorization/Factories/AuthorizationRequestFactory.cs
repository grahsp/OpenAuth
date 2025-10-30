using OpenAuth.Application.OAuth.Authorization.Dtos;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Shared.Models;
using OpenAuth.Domain.AuthorizationGrants.Enums;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Factories;

public class AuthorizationRequestFactory : IAuthorizationRequestFactory
{
    public Result<AuthorizationRequest> CreateFrom(IAuthorizeQuery query)
    {
        // TODO: Add support for other response types, e.g. token, id_token.
        if (!string.Equals(query.ResponseType, "code", StringComparison.OrdinalIgnoreCase))
            return Result<AuthorizationRequest>.Fail(new Error("unsupported_response_type", "Only 'code' response type is supported."));

        if (!ClientId.TryCreate(query.ClientId, out var clientId))
            return Result<AuthorizationRequest>.Fail(new Error("invalid_request", "Invalid Client ID."));

        if (!RedirectUri.TryCreate(query.RedirectUri, out var redirectUri))
            return Result<AuthorizationRequest>.Fail(new Error("invalid_request", "Invalid redirect URI."));
        
        if (!AudienceName.TryCreate(query.Audience, out var audience))
            return Result<AuthorizationRequest>.Fail(new Error("invalid_request", "Invalid audience."));

        var scopes = Scope.ParseMany(query.Scope);
        
        var pkceResult = ValidatePkce(query.CodeChallenge, query.CodeChallengeMethod);
        if (!pkceResult.IsSuccess)
            return Result<AuthorizationRequest>.Fail(pkceResult.Error!);

        var request = new AuthorizationRequest
        (
            clientId,
            GrantType.AuthorizationCode,
            redirectUri,
            audience,
            scopes,
            pkceResult.Value
        );

        return Result<AuthorizationRequest>.Ok(request);
    }
    
    private static Result<Pkce?> ValidatePkce(string? codeChallenge, string? codeChallengeMethod)
    {
        if (codeChallenge is null)
            return Result<Pkce?>.Ok(null);
        
        if (codeChallengeMethod is null)
            return Result<Pkce?>.Fail(new Error("invalid_request", "Code challenge method is required."));
        
        if (!Enum.TryParse(codeChallengeMethod, out CodeChallengeMethod method))
            return Result<Pkce?>.Fail(new Error("invalid_request", "Invalid code challenge method."));
        
        return Result<Pkce?>.Ok(Pkce.Create(codeChallenge, method));
    }
}