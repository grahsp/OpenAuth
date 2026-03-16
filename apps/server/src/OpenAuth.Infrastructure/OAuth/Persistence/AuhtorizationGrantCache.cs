using System.Collections.Concurrent;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Domain.AuthorizationGrants;

namespace OpenAuth.Infrastructure.OAuth.Persistence;

public class AuthorizationGrantCache : ICache<AuthorizationGrant>
{
    private readonly ConcurrentDictionary<string, AuthorizationGrant> _cache = [];

    
    public Task<AuthorizationGrant?> GetAsync(string key)
    {
        _cache.TryGetValue(key, out var entry);
        return Task.FromResult(entry);
    }

    public Task SetAsync(string key, AuthorizationGrant grant, TimeSpan? expiration = null)
    {
        _cache[key] = grant;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}