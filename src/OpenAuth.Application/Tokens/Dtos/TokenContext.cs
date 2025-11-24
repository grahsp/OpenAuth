namespace OpenAuth.Application.Tokens.Dtos;

public record TokenContext(
    string ClientId,
    string? Subject,
    string Audience,
    IReadOnlyCollection<string> ApiScopes,
    OidcContext? OidcContext = null
);