namespace OpenAuth.Infrastructure.OAuth.Jwt;

public record AccessTokenResult(string Token, TokenType TokenType, int ExpiresIn);