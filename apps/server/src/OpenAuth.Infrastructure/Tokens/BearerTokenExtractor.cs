using Microsoft.AspNetCore.Http;
using OpenAuth.Application.Tokens.Interfaces;

namespace OpenAuth.Infrastructure.Tokens;

public class BearerTokenExtractor(IHttpContextAccessor accessor) : IBearerTokenExtractor
{
    public string? ExtractToken()
    {
        var header = accessor.HttpContext?.Request.Headers.Authorization;
        if (header is null)
            return null;

        var value = header.ToString();
        if (value is null || !value.StartsWith("Bearer "))
            return null;

        return value["Bearer ".Length..];
    }
}