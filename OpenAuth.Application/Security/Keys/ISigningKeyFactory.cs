using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Application.Security.Keys;

public interface ISigningKeyFactory
{
    SigningKey Create(SigningAlgorithm algorithm, DateTime? expiresAt = null);
}