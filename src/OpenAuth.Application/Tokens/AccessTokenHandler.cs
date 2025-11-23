using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Application.Tokens;

public class AccessTokenHandler : ITokenHandler<AccessTokenContext>
{
    private readonly IJwtBuilderFactory _builderFactory;
    private readonly IJwtSigner _jwtSigner;
    
    public AccessTokenHandler(IJwtBuilderFactory builderFactory, IJwtSigner jwtSigner)
    {
        _builderFactory = builderFactory;
        _jwtSigner = jwtSigner;
    }
    
    public async Task<string> CreateAsync(AccessTokenContext context, CancellationToken ct = default)
    {
        var builder = _builderFactory.Create()
            .AddClaim(OAuthClaimTypes.ClientId, context.TokenData.Id.ToString())
            .AddClaim(OAuthClaimTypes.Sub, context.Subject)
            .AddClaim(OAuthClaimTypes.Aud, context.Audience.Value)
            .WithLifetime(context.TokenData.TokenLifetime);

        foreach (var scope in context.ApiScopes)
            builder.AddClaim(OAuthClaimTypes.Scope, scope.Value);

        var descriptor = builder.Build();
        var token = await _jwtSigner.Create(descriptor, ct);

        return token;
    }
}