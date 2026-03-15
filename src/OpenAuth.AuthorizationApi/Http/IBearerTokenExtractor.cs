namespace OpenAuth.AuthorizationApi.Http;

public interface IBearerTokenExtractor
{
    string? TryExtract(HttpRequest request);
}