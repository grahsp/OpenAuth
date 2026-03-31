namespace OpenAuth.Application.Clients.Dtos;

public record ClientInfo(
    string Id,
    string Name,
    string ApplicationType,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);