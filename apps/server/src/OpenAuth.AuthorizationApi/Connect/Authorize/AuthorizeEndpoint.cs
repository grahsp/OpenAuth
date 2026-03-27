using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Handlers;

namespace OpenAuth.AuthorizationApi.Connect.Authorize;

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
			var isAuthenticated = user.Identity?.IsAuthenticated == true;

			if (!isAuthenticated)
			{
				if (IsSilent(dto))
					return ErrorRedirect(dto.RedirectUri, "login_required", "User not authenticated", dto.State);
				
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
				return ErrorRedirect(dto.RedirectUri, ex.Error, ex.Description, dto.State);
			}
			catch (OAuthAuthorizationException ex)
			{
				return ErrorRedirect(dto.RedirectUri, ex.Error, ex.Description, dto.State);
			}
			catch (Exception)
			{
				return Results.InternalServerError("An unexpected error occurred.");
			}    
		});

		return app;
	}
    
	private static bool IsSilent(AuthorizeRequest dto)
		=> dto.Prompt == "none";

	private static IResult ErrorRedirect(string redirectUri, string error, string? description, string? state)
	{
		var parameters = new Dictionary<string, string?>
		{
			["error"] = error,
			["error_description"] = description,
			["state"] = state
		};

		var uri = QueryHelpers.AddQueryString(redirectUri, parameters);
		return Results.Redirect(uri);
	}
}