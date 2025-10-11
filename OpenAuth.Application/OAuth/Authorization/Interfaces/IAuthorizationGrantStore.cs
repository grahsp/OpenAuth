using OpenAuth.Domain.AuthorizationGrant;

namespace OpenAuth.Application.OAuth.Authorization.Interfaces;

public interface IAuthorizationGrantStore
{
    Task<AuthorizationGrant?> GetAsync(string code);
    Task AddAsync(AuthorizationGrant grant);
    Task RemoveAsync(string code);
}