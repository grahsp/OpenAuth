namespace OpenAuth.Api.Dtos;

public record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);