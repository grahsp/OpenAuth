using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record ClientDetails(
    ClientId Id,
    ClientName Name,
    DateTimeOffset CreatedAt,
    IEnumerable<ClientSecretDto> Secrets,
    IEnumerable<AudienceSummary> Audiences
);