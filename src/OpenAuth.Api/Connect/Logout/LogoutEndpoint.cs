using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OpenAuth.Domain.Users;

namespace OpenAuth.Api.Connect.Logout;

public static class LogoutEndpoint
{
    public static IEndpointRouteBuilder MapLogoutEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/connect/logout", async (
            [FromServices] SignInManager<User> signinManager,
            [AsParameters] LogoutRequest query) =>
        {
            await signinManager.SignOutAsync();

            if (string.IsNullOrEmpty(query.RedirectUri))
                return Results.Ok("Logged out.");
            
            var redirectUri = QueryHelpers.AddQueryString(
                query.RedirectUri,
                new Dictionary<string, string?>
                {
                    ["state"] = query.State
                });
                    
            return Results.Redirect(redirectUri);
        });
        
        return app;
    }
}