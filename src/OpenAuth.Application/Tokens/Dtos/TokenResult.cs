namespace OpenAuth.Application.Tokens.Dtos;

public record TokenResult(
    string Token,
    string TokenType,
    int ExpiresIn,
    string? IdToken = null,
    string? RefreshToken = null,
    string? Scope = null
);