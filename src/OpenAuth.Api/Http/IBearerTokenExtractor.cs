namespace OpenAuth.Api.Http;

public interface IBearerTokenExtractor
{
    string? TryExtract(HttpRequest request);
}