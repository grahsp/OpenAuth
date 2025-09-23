using OpenAuth.Api.Controllers;
using OpenAuth.Api.Dtos;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Api.Mappers;

public static class SigningKeyMapper
{
    public static SigningKeyResponse ToResponse(SigningKey key)
        => new SigningKeyResponse(
            key.Id.Value,
            key.Algorithm.ToString(),
            key.IsActive(),
            key.CreatedAt,
            key.ExpiresAt,
            key.RevokedAt
        );

    public static Jwk ToJwk(SigningKey key, KeyParameters parameters)
    {
        return new Jwk
        {
            Kid = key.Id.Value.ToString(),
            Kty = "RSA",
            Alg = "RS256",
            Use = "sig",
            N = parameters.N,
            E = parameters.E,
        };
    }
}