using OpenAuth.Application.SigningKeys.Factories;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Fakes;

public class FakeSigningKeyFactory : ISigningKeyFactory
{
    public SigningKey Create(SigningAlgorithm algorithm, DateTimeOffset createdAt, TimeSpan? lifetime = null)
        => algorithm switch
        {
            SigningAlgorithm.RS256 => TestSigningKey.CreateRsaSigningKey(createdAt: createdAt, expiresAt: createdAt.Add(lifetime ?? TimeSpan.FromDays(30))),
            SigningAlgorithm.HS256 => TestSigningKey.CreateHmacSigningKey(createdAt: createdAt, expiresAt: createdAt.Add(lifetime ?? TimeSpan.FromDays(30))),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
}