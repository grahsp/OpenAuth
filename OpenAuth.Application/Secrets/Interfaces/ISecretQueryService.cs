using OpenAuth.Application.Secrets.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Secrets.Interfaces;

public interface ISecretQueryService
{
    Task<bool> ValidateSecretAsync(ClientId clientId, string plainSecret, CancellationToken ct = default);
    Task<IReadOnlyCollection<SecretInfo>> GetActiveSecretsAsync(ClientId clientId, CancellationToken ct = default);
}