using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.Tokens;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Application.Oidc;

public class IdTokenHandler : ITokenHandler<IdTokenContext>
{
    private readonly IOidcUserClaimsProvider _oidcUserClaims;
    private readonly IJwtBuilderFactory _builderFactory;
    private readonly IJwtSigner _jwtSigner;
    
    public IdTokenHandler(IOidcUserClaimsProvider oidcUserClaims, IJwtBuilderFactory builderFactory, IJwtSigner jwtSigner)
    {
        _oidcUserClaims = oidcUserClaims;
        _builderFactory = builderFactory;
        _jwtSigner = jwtSigner;
    }

    public async Task<string> CreateAsync(IdTokenContext context, CancellationToken ct = default)
    {
        var builder = _builderFactory.Create()
            .AddClaim(OAuthClaimTypes.Sub, context.AuthorizationGrant.Subject)
            .AddClaim(OAuthClaimTypes.Aud, context.AuthorizationGrant.ClientId.ToString())
            .AddClaim("auth_time", context.AuthorizationGrant.CreatedAt.ToUnixTimeSeconds().ToString())
            .AddOptionalClaim("nonce", context.AuthorizationGrant.Nonce)
            // TODO: add IdTokenLifetime?
            .WithLifetime(context.TokenData.TokenLifetime);

        var userClaims = await _oidcUserClaims
            .GetUserClaimsAsync(context.AuthorizationGrant.Subject, context.OidcScopes);
        
        builder.AddClaims(userClaims);

        var descriptor = builder.Build();
        var token = await _jwtSigner.Create(descriptor, ct);

        return token;
    }
}