using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Handlers;

namespace OpenAuth.Api.Identity.Authorize;

public static class AuthorizeEndpoint
{
    public static IEndpointRouteBuilder MapAuthorizeEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/connect/authorize", async (
            IAuthorizationHandler handler,
            HttpContext http,
            HttpRequest request,
            [AsParameters] AuthorizeRequest dto) =>
        {
            if (!dto.Validate(out var error, out var description))
                return Results.BadRequest(new { error = error, error_description = description });
            
            var user = http.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                var redirectUri = request.Path + request.QueryString;
                return Results.Challenge(new AuthenticationProperties { RedirectUri = redirectUri });
            }

            var subject = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(subject))
                return Results.BadRequest("Subject is missing.");

            try
            {
                var command = dto.ToCommand(subject);
                
                var grant = await handler.HandleAsync(command);
                var redirectUri = grant.ToRedirectUri(dto.State);

                return Results.Redirect(redirectUri);
            }
            catch (OAuthProtocolException ex)
            {
                return Results.BadRequest(new { error = ex.Error, description = ex.Description });
            }
            catch (OAuthAuthorizationException ex)
            {
                var parameters = new Dictionary<string, string?>
                {
                    ["error"] = ex.Error,
                    ["error_description"] = ex.Description,
                    ["state"] = dto.State
                };

                var redirectUri = QueryHelpers.AddQueryString(
                    dto.RedirectUri,
                    parameters
                );
                
                return Results.Redirect(redirectUri);
            }
            catch (Exception)
            {
                return Results.InternalServerError("An unexpected error occurred.");
            }    
        });

        return app;
    }
}