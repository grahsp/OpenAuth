using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record ClientDetails(
    ClientId Id,
    ClientName Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IEnumerable<SecretInfo> Secrets,
    IEnumerable<AudienceInfo> Audiences
);