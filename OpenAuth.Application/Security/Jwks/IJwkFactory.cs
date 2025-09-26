using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Jwks;

public interface IJwkFactory
{
    Jwk Create(string kid, KeyMaterial keyMaterial);
}