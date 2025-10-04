using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public class ClientCreationResult
(
    ClientId Id,
    ClientName Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    SecretCreationResult InitialSecret
);