namespace OpenAuth.Api.Http;

public class BearerTokenExtractor : IBearerTokenExtractor
{
    public string? TryExtract(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("Authorization", out var authHeader))
            return null;

        const string prefix = "Bearer ";
        var header = authHeader.ToString();

        return header.StartsWith(prefix)
            ? header[prefix.Length..]
            : null;
    }
}