using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record ClientInfo(
    ClientId Id,
    ClientName Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);