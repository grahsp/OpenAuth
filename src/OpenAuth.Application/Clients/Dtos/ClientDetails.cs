using OpenAuth.Application.Clients.Mappings;
using OpenAuth.Application.Secrets.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public record ClientDetails(
    ClientId Id,
    ClientName Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IEnumerable<SecretInfo> Secrets,
    IEnumerable<ClientApiAccessDetails> ApiAccess
);