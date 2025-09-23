using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Common.Fakes;

public class FakeSigningKeyFactory : ISigningKeyFactory
{
    public SigningKey Create(SigningAlgorithm algorithm, DateTime? expiresAt = null) =>
        new SigningKey(algorithm, new Key("private-key"), expiresAt);
}