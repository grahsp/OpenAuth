using OpenAuth.Application.Jwks.Services;

namespace OpenAuth.Api.Connect.Jwks;

public static class JwksEndpoint
{
    public static IEndpointRouteBuilder MapJwksEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/.well-known/jwks", async (IJwksService service) =>
        {
            var publicKeyInfo = await service.GetJwksAsync();
            var response = publicKeyInfo.ToJwkSet();
        
            return Results.Ok(response);
        });
        
        return app;
    }
}