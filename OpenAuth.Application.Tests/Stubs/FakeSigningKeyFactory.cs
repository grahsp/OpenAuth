using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Application.Tests.Stubs;

public class FakeSigningKeyFactory : ISigningKeyFactory
{
    public SigningKey Create(SigningAlgorithm algorithm, DateTime? expiresAt = null) =>
        SigningKey.CreateAsymmetric(
            algorithm,
            "public-key",
            "private-key",
            expiresAt
        );
}