using OpenAuth.Application.Exceptions;
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
            .AddClaim(OAuthClaimTypes.Aud, context.ClientId)
            .AddClaim(OAuthClaimTypes.Sub, context.Subject)
            .AddClaim("auth_time", context.AuthTimeInSeconds.ToString())
            .AddOptionalClaim("nonce", context.Nonce)
            // TODO: add IdTokenLifetime?
            .WithLifetime(context.LifetimeInSeconds);

        var userClaims = await _oidcUserClaims
            .GetUserClaimsAsync(context.Subject, context.Scopes)
            ?? throw new InvalidClientException("Client not found.");
        
        builder.AddClaims(userClaims);

        var descriptor = builder.Build();
        var token = await _jwtSigner.Create(descriptor, ct);

        return token;
    }
}