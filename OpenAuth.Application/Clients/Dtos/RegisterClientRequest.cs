using OpenAuth.Domain.Clients.Enums;

namespace OpenAuth.Application.Clients.Dtos;

public sealed record RegisterClientRequest(
    ClientApplicationType Type,
    string Name,
    Dictionary<string, IEnumerable<string>>? Permissions = null,
    IEnumerable<string>? RedirectUris = null
);