using Microsoft.AspNetCore.Mvc;
using OpenAuth.Application.OAuth.Services;
using OpenAuth.Application.Tokens.Interfaces;

namespace OpenAuth.AuthorizationApi.Connect.UserInfo;

public static class UserInfoEndpoint
{
    public static IEndpointRouteBuilder MapUserInfoEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/connect/userinfo", async (
            [FromServices] IBearerTokenExtractor extractor,
            [FromServices] IUserInfoService service) =>
        {
            var token = extractor.ExtractToken();
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