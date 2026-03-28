using Microsoft.Extensions.Options;
using OpenAuth.Application.Tokens.Configurations;

namespace OpenAuth.Server.Discovery.Discovery;

public static class OpenIdConfigurationEndpoint
{
    public static IEndpointRouteBuilder MapOpenIdConfigurationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/openid-configuration", (IOptions<JwtOptions> config) =>
        {
            var cfg = config.Value;

            return new OpenIdConfiguration(
                cfg.Issuer,
                $"{cfg.Issuer}/connect/authorize",
                $"{cfg.Issuer}/connect/token",
                $"{cfg.Issuer}/connect/logout",
                $"{cfg.Issuer}/connect/userinfo",
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