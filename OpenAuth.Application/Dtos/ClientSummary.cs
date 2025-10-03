using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record ClientSummary(
    ClientId Id,
    ClientName Name,
    DateTimeOffset CreatedAt
);