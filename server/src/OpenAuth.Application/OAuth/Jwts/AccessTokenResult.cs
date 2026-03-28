namespace OpenAuth.Application.OAuth.Jwts;

public record AccessTokenResult(string Token, TokenType TokenType, int ExpiresIn);