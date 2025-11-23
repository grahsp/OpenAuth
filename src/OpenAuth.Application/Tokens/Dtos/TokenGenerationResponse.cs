namespace OpenAuth.Application.Tokens.Dtos;

public record TokenGenerationResponse(
    string Token,
    string TokenType,
    int ExpiresIn
);