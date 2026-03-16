using Microsoft.AspNetCore.WebUtilities;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.AuthorizationApi.Connect.Authorize;

public static class AuthorizeMappings
{
	public static AuthorizeCommand ToCommand(this AuthorizeRequest request, string subject)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(request.ResponseType, nameof(request.ResponseType));
		ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        
		if (!ClientId.TryParse(request.ClientId, out var id))
			throw new MalformedClientException("Invalid client_id parameter.");
        
		if (!RedirectUri.TryParse(request.RedirectUri, out var uri))
			throw new MalformedRedirectUriException("Invalid redirect_uri parameter.");
        
		if (!AudienceIdentifier.TryParse(request.Audience, out var audience))
			throw new InvalidAudienceException("Invalid audience parameter.");

		if (!ScopeCollection.TryParse(request.Scopes, out var scope))
			throw new MalformedScopeException("Invalid scope parameter.");
        
		if (!Pkce.TryParse(request.CodeChallenge, request.CodeChallengeMethod, out var pkce))
			throw new MalformedPkceException("Invalid PKCE parameters.");

		return AuthorizeCommand.Create(
			request.ResponseType,
			id,
			subject,
			uri,
			audience,
			scope,
			pkce,
			request.Nonce
		);
	}
    
	public static string ToRedirectUri(this AuthorizationGrant grant, string? state)
		=> QueryHelpers.AddQueryString(
			grant.RedirectUri.Value,
			new Dictionary<string, string?>
			{
				["code"] = grant.Code,
				["state"] = state
			});
}