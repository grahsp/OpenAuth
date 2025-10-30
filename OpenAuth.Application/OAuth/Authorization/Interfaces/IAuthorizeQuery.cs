namespace OpenAuth.Application.OAuth.Authorization.Interfaces;

public interface IAuthorizeQuery
{
    string ResponseType { get; }
    string ClientId { get; }
    string RedirectUri { get; }
    string Audience { get; }
    string? Scope { get; }
    string? State { get; }
    string? CodeChallenge { get; }
    string? CodeChallengeMethod { get; }
}