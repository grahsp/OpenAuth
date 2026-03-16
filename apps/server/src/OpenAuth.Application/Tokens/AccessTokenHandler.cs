using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Application.Tokens;

public class AccessTokenHandler : ITokenHandler<AccessTokenContext>
{
    private readonly IJwtSigner _jwtSigner;
    
    public AccessTokenHandler(IJwtSigner jwtSigner)
    {
        _jwtSigner = jwtSigner;
    }
    
    public async Task<string> CreateAsync(AccessTokenContext context, CancellationToken ct = default)
    {
        var claims = new Dictionary<string, object>
        {
            { "scope", context.Scope.ToString() },
        };
        
        if (context.ClientId is not null)
            claims.Add("client_id", context.ClientId);

        var descriptor = new JwtDescriptor(
            context.Audience,
            context.Subject,
            context.LifetimeInSeconds,
            claims
        );
        
        var token = await _jwtSigner.Create(descriptor, ct);

        return token;
    }
}