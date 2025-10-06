using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Application.Security.Keys;

public interface ISigningKeyFactory
{
    SigningKey Create(SigningAlgorithm algorithm, DateTimeOffset createdAt, TimeSpan? lifetime = null);
}