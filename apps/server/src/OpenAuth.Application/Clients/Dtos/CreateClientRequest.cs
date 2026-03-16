using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Dtos;

public sealed record CreateClientRequest(
    ClientApplicationType ApplicationType,
    ClientName Name,
    IEnumerable<RedirectUri> RedirectUris
);