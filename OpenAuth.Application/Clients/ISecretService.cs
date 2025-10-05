using OpenAuth.Application.Dtos;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Clients;

public interface ISecretService
{
    Task<SecretCreationResult> AddSecretAsync(ClientId clientId, CancellationToken cancellationToken = default);
    Task RevokeSecretAsync(ClientId clientId, SecretId secretId, CancellationToken cancellationToken = default);
}