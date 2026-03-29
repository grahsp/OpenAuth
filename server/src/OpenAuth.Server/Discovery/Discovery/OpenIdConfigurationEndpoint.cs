using Microsoft.Extensions.Options;
using OpenAuth.Application.Tokens.Configurations;

namespace OpenAuth.Server.Discovery.Discovery;

public static class OpenIdConfigurationEndpoint
{
    public static RouteHandlerBuilder MapOpenIdConfigurationEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/openid-configuration", (IOptions<OAuthOptions> config) =>
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
    }
}