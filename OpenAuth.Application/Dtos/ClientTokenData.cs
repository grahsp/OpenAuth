using OpenAuth.Domain.Clients.Audiences.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record ClientTokenData(Scope[] AllowedScopes, TimeSpan TokenLifetime);