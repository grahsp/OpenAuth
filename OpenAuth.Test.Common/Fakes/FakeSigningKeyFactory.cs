using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
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