using Microsoft.Extensions.Options;
using OpenAuth.Application.Tokens.Configurations;

namespace OpenAuth.Api.Connect.Discovery;

public static class DiscoveryEndpoint
{
    public static IEndpointRouteBuilder MapDiscoveryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/.well-known/openid-configuration", (IOptions<JwtOptions> config) =>
        {
            var cfg = config.Value;

            return new DiscoveryDocument(
                cfg.Issuer,
                $"{cfg.Issuer}/connect/authorize",
                $"{cfg.Issuer}/connect/token",
                "",
                $"{cfg.Issuer}/.well-known/jwks.json",
                ["code"],
                ["public"],
                ["RS256"],
                ["openid", "profile"],
                ["sub", "email", "phone"]
            );
        });
        
        return app;
    }
}