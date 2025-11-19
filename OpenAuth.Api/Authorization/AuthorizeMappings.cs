using Microsoft.AspNetCore.WebUtilities;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.AuthorizationGrants;

namespace OpenAuth.Api.Authorization;

public static class AuthorizeMappings
{
    public static AuthorizeCommand ToCommand(this AuthorizeRequestDto request, string subject)
        => new(
            request.ResponseType,
            request.ClientId,
            subject,
            request.RedirectUri,
            "", // TODO: audience not required during authorization
            request.Scope,
            request.State,
            request.CodeChallenge,
            request.CodeChallengeMethod);
    
    public static string ToRedirectUri(this AuthorizationGrant grant, string? state)
        => QueryHelpers.AddQueryString(
            grant.RedirectUri.Value,
            new Dictionary<string, string?>
            {
                ["code"] = grant.Code,
                ["state"] = state
            });
}