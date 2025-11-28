using Microsoft.AspNetCore.WebUtilities;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.AuthorizationGrants;

namespace OpenAuth.Api.Identity.Authorize;

public static class AuthorizeMappings
{
    public static AuthorizeCommand ToCommand(this AuthorizeRequest request, string subject)
        => AuthorizeCommand.Create(
            request.ResponseType,
            request.ClientId,
            subject,
            request.RedirectUri,
            request.Scopes,
            request.CodeChallenge,
            request.CodeChallengeMethod,
            request.Nonce);
    
    public static string ToRedirectUri(this AuthorizationGrant grant, string? state)
        => QueryHelpers.AddQueryString(
            grant.RedirectUri.Value,
            new Dictionary<string, string?>
            {
                ["code"] = grant.Code,
                ["state"] = state
            });
}