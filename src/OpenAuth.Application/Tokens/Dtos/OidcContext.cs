namespace OpenAuth.Application.Tokens.Dtos;

public record OidcContext(
    string Nonce,
    int AuthTimeInSeconds,
    IReadOnlyCollection<string>? Scopes
);