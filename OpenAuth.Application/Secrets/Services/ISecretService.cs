using OpenAuth.Application.Secrets.Dtos;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Secrets.Services;

public interface ISecretService
{
    Task<SecretCreationResult> AddSecretAsync(ClientId clientId, CancellationToken cancellationToken = default);
    Task RevokeSecretAsync(ClientId clientId, SecretId secretId, CancellationToken cancellationToken = default);
}