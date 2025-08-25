using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Application.Security.Keys;

public interface ISigningKeyStrategy
{
    SigningAlgorithm Algorithm { get; }
    SigningKey Create(DateTime? expiresAt = null);
}