using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Domain.AuthorizationGrants;

namespace OpenAuth.Application.OAuth.Stores;

public class AuthorizationGrantStore : IAuthorizationGrantStore
{
    private readonly ICache<AuthorizationGrant> _cache;

    public AuthorizationGrantStore(ICache<AuthorizationGrant> cache)
    {
        _cache = cache;
    }
    
    
    public async Task<AuthorizationGrant?> GetAsync(string code)
    {
        return await _cache.GetAsync(code);
    }

    public async Task AddAsync(AuthorizationGrant grant)
    {
        await _cache.SetAsync(grant.Code, grant, TimeSpan.FromMinutes(10));
    }

    public async Task RemoveAsync(string code)
    {
        await _cache.DeleteAsync(code);
    }
}