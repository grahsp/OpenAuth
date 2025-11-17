namespace OpenAuth.Application.Clients.Dtos;

public sealed record RegisterClientCommand(
    string ApplicationType,
    string Name,
    Dictionary<string, IEnumerable<string>>? Permissions = null,
    IEnumerable<string>? RedirectUris = null
);