using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
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
    
    Task<Client> GrantScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);
    Task<Client> RevokeScopesAsync(ClientId id, Audience audience, IEnumerable<Scope> scopes, CancellationToken cancellationToken = default);
    Task<Client> RemoveAudienceAsync(ClientId id, Audience audience, CancellationToken cancellationToken = default);
    
    Task<string> AddSecretAsync(ClientId id, DateTime? expiresAt = null, CancellationToken cancellationToken = default);
    Task<bool> RevokeSecretAsync(SecretId secretId, CancellationToken cancellationToken = default);
    Task<bool> RemoveSecretAsync(SecretId secretId, CancellationToken cancellationToken = default);
    
    Task<SigningKey> AddSigningKeyAsync(ClientId id, SigningAlgorithm algorithm, DateTime? expiresAt = null, CancellationToken cancellationToken = default);
    Task<bool> RevokeSigningKeyAsync(ClientId clientId, SigningKeyId keyId, CancellationToken cancellationToken = default);
    Task<bool> RemoveSigningKeyAsync(ClientId clientId, SigningKeyId keyId, CancellationToken cancellationToken = default);
}