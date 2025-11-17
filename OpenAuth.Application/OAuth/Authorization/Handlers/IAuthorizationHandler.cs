using OpenAuth.Domain.AuthorizationGrants;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public interface IAuthorizationHandler
{
    Task<AuthorizationGrant> AuthorizeAsync(AuthorizeCommand cmd, CancellationToken ct = default);
}