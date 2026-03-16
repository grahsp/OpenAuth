namespace OpenAuth.Application.OAuth.Authorization.Interfaces;

public interface ICache<T> where T : class
{
    Task<T?> GetAsync(string key);
    Task SetAsync(string key, T value, TimeSpan? expiration = null);
    Task DeleteAsync(string key);
}