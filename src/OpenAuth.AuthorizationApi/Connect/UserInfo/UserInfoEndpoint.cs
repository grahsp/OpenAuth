using Microsoft.AspNetCore.Mvc;
using OpenAuth.Application.OAuth.Services;
using OpenAuth.AuthorizationApi.Http;

namespace OpenAuth.AuthorizationApi.Connect.UserInfo;

public static class UserInfoEndpoint
{
    public static IEndpointRouteBuilder MapUserInfoEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/connect/userinfo", async (
            HttpRequest request,
            [FromServices] IBearerTokenExtractor extractor,
            [FromServices] IUserInfoService service) =>
        {
            var token = extractor.TryExtract(request);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Unauthorized();
            
            var claims = await service.GetUserClaimsAsync(token);
            var response = new Dictionary<string, object>();

            foreach (var claim in claims)
                response[claim.Type] = claim.Value;
            
            return Results.Ok(response);
        });
        
        return app;
    }
}