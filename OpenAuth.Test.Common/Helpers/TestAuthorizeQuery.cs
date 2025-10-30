using OpenAuth.Application.OAuth.Authorization.Interfaces;

namespace OpenAuth.Test.Common.Helpers;

public record TestAuthorizeQuery : IAuthorizeQuery
{
    public required string ResponseType { get; init; }
    public required string ClientId { get; init; }
    public required string RedirectUri { get; init; }
    public required string Audience { get; init; }
    public string? Scope { get; init; }
    public string? State { get; init; }
    public string? CodeChallenge { get; init; }
    public string? CodeChallengeMethod { get; init; }
    
    public static TestAuthorizeQuery CreateDefault()
    {
        return new TestAuthorizeQuery
        {
            ResponseType = "code",
            ClientId = Guid.NewGuid().ToString(),
            RedirectUri = "https://example.com/callback",
            Audience = "api.example.com",
            Scope = "read write",
            State = "123456"
        };
    }
}