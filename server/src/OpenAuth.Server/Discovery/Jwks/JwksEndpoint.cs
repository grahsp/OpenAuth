using OpenAuth.Application.Jwks.Services;

namespace OpenAuth.Server.Discovery.Jwks;

public static class JwksEndpoint
{
    public static RouteHandlerBuilder MapJwksEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/jwks.json", async (IJwksService service) =>
        {
            var publicKeyInfo = await service.GetJwksAsync();
            var response = publicKeyInfo.ToJwkSet();
        
            return Results.Ok(response);
        });
    }
}