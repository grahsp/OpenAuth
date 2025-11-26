namespace OpenAuth.Application.Tokens.Dtos;

public record TokenResult(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string? IdToken = null,
    string? RefreshToken = null,
    string? Scope = null
);