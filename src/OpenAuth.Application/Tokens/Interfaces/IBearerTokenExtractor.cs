namespace OpenAuth.Application.Tokens.Interfaces;

public interface IBearerTokenExtractor
{
    string? ExtractToken();
}