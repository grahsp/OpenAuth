using OpenAuth.Application.Dtos;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Queries;

public interface ISecretQueryService
{
    Task<bool> ValidateSecretAsync(ClientId clientId, string plainSecret, CancellationToken ct = default);
    Task<IEnumerable<SecretInfo>> GetActiveSecretsAsync(ClientId clientId, CancellationToken ct = default);
}