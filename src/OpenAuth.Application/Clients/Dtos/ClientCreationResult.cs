using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.Services.Dtos;

namespace OpenAuth.Application.Clients.Dtos;

public class ClientCreationResult
(
    ClientId Id,
    ClientName Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    SecretCreationResult InitialSecret
);