using OpenAuth.Domain.AuthorizationGrant;

namespace OpenAuth.Application.OAuth.Authorization.Flows;

public interface IAuthorizationGrantStore
{
    Task<AuthorizationGrant?> GetAsync(string code);
    Task AddAsync(AuthorizationGrant grant);
    Task RemoveAsync(string code);
}