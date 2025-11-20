namespace OpenAuth.Test.Common.ValueObjects;

public record AuthorizeResult(bool Success, string? Code, string? State, string? RedirectUri, string? Error = null);