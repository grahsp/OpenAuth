using System.IdentityModel.Tokens.Jwt;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.Tokens;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Application.Oidc;

public class IdTokenHandler : ITokenHandler<IdTokenContext>
{
    private readonly IOidcUserClaimsProvider _oidcUserClaims;
    private readonly IJwtSigner _jwtSigner;
    
    public IdTokenHandler(IOidcUserClaimsProvider oidcUserClaims, IJwtSigner jwtSigner)
    {
        _oidcUserClaims = oidcUserClaims;
        _jwtSigner = jwtSigner;
    }

    public async Task<string> CreateAsync(IdTokenContext context, CancellationToken ct = default)
    {
        var userClaims = await _oidcUserClaims
                             .GetUserClaimsAsync(context.Subject, context.Scopes)
                         ?? throw new InvalidClientException("Client not found.");

        var claims = new Dictionary<string, object>
        {
            { JwtRegisteredClaimNames.AuthTime, context.AuthTimeInSeconds },
        };

        foreach (var claim in userClaims)
            claims[claim.Type] = claim.Value;

        var descriptor = new JwtDescriptor(
            context.ClientId,
            context.Subject,
            context.LifetimeInSeconds,
            claims
        );
        
        var token = await _jwtSigner.Create(descriptor, ct);

        return token;
    }
}