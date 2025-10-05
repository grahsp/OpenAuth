namespace OpenAuth.Application.Dtos;

public record TokenGenerationResponse(
    string Token,
    string TokenType,
    int ExpiresIn
);