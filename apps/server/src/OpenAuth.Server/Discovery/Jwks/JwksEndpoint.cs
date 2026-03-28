using OpenAuth.Application.Jwks.Services;

namespace OpenAuth.Server.Discovery.Jwks;

public static class JwksEndpoint
{
    public static IEndpointRouteBuilder MapJwksEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/jwks.json", async (IJwksService service) =>
        {
            var publicKeyInfo = await service.GetJwksAsync();
            var response = publicKeyInfo.ToJwkSet();
        
            return Results.Ok(response);
        });
        
        return app;
    }
}