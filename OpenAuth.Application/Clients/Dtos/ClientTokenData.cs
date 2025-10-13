using OpenAuth.Domain.Clients.Audiences.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public record ClientTokenData(IEnumerable<Scope> AllowedScopes, TimeSpan TokenLifetime);