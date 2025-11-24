using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record TokenContext(
    ClientId ClientId,
    string? Subject,
    string? Audience,
    IReadOnlyCollection<string>? ApiScopes,
    OidcContext? OidcContext = null
);