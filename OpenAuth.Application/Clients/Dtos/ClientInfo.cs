using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public record ClientInfo(
    ClientId Id,
    ClientName Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);