using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.Tokens;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Application.Oidc;

public class IdTokenHandler : ITokenHandler<IdTokenContext>
{
    private readonly IUserClaimsQueryService _userClaims;
    private readonly IJwtBuilderFactory _builderFactory;
    private readonly IJwtSigner _jwtSigner;
    
    public IdTokenHandler(IUserClaimsQueryService userClaims, IJwtBuilderFactory builderFactory, IJwtSigner jwtSigner)
    {
        _userClaims = userClaims;
        _builderFactory = builderFactory;
        _jwtSigner = jwtSigner;
    }

    public async Task<string> CreateAsync(IdTokenContext context, CancellationToken ct = default)
    {
        var builder = _builderFactory.Create()
            .AddClaim(OAuthClaimTypes.Sub, context.AuthorizationGrant.Subject)
            .AddClaim(OAuthClaimTypes.Aud, context.Client.Id.ToString())
            .AddClaim("auth_time", context.AuthorizationGrant.CreatedAt.ToUnixTimeSeconds().ToString())
            .AddOptionalClaim("nonce", context.AuthorizationGrant.Nonce)
            // TODO: add IdTokenLifetime?
            .WithLifetime(context.Client.TokenLifetime);

        var userClaims = await _userClaims
            .GetUserClaimsAsync(context.AuthorizationGrant.Subject, context.OidcScopes);
        
        builder.AddClaims(userClaims);

        var descriptor = builder.Build();
        var token = await _jwtSigner.Create(descriptor, ct);

        return token;
    }
}