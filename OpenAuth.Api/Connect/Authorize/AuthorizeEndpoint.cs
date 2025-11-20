using Microsoft.AspNetCore.Authentication;
using OpenAuth.Application.OAuth.Authorization.Handlers;

namespace OpenAuth.Api.Connect.Authorize;

public static class AuthorizeEndpoint
{
    public static IEndpointRouteBuilder MapAuthorizeEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/connect/authorize", async (
            IAuthorizationHandler handler,
            HttpContext http,
            HttpRequest request,
            [AsParameters] AuthorizeRequestDto dto) =>
        {
            if (!dto.Validate(out var error, out var description))
                return Results.BadRequest(new { error = error, error_description = description });
            
            var user = http.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                var redirectUri = request.Path + request.QueryString;
                return Results.Challenge(new AuthenticationProperties { RedirectUri = redirectUri });
            }

            var subject = user.Identity.Name;
            if (string.IsNullOrWhiteSpace(subject))
                return Results.BadRequest("Subject is missing.");
        
            var command = dto.ToCommand(subject);

            try
            {
                var grant = await handler.AuthorizeAsync(command);
                var redirectUri = grant.ToRedirectUri(dto.State);
        
                return Results.Redirect(redirectUri);
            }
            catch (Exception ex)
            {
                return Results.InternalServerError("An unexpected error occurred.");
            }    
        });

        return app;
    }
}