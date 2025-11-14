namespace OpenAuth.Application.Clients.Dtos;

public record ClientInfo(
    string Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);