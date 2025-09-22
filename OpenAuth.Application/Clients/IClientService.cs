using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Clients;

public interface IClientService
{
    Task<Client?> GetByIdAsync(ClientId id, CancellationToken cancellationToken = default);
    Task<Client?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    Task<Client> RegisterAsync(string name, CancellationToken cancellationToken = default);
    Task<Client> RenameAsync(ClientId id, string name, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(ClientId id, CancellationToken cancellationToken = default);
    
    Task<Client> EnableAsync(ClientId id, CancellationToken cancellationToken = default);
    Task<Client> DisableAsync(ClientId id, CancellationToken cancellationToken = default);
    
    Task<Client?> TryAddAudienceAsync(ClientId id, Audience audience, CancellationToken cancellationToken = default);
    Task<Client?> TryRemoveAudienceAsync(ClientId id, Audience audience, CancellationToken cancellationToken = default);
    Task<Client> SetScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);
    Task<Client> GrantScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);
    Task<Client> RevokeScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);
    
    Task<ClientSecret?> GetSecretAsync(SecretId id, CancellationToken cancellationToken = default);
    Task<SecretCreationResult> AddSecretAsync(ClientId id, DateTime? expiresAt = null, CancellationToken cancellationToken = default);
    Task<bool> RevokeSecretAsync(SecretId secretId, CancellationToken cancellationToken = default);
    Task<bool> RemoveSecretAsync(SecretId secretId, CancellationToken cancellationToken = default);
}