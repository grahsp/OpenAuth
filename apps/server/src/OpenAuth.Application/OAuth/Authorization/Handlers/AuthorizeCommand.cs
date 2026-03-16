using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public record AuthorizeCommand
{
    public string ResponseType { get; init; }
    public ClientId ClientId { get; init; }
    public string Subject { get; init; }
    public RedirectUri RedirectUri { get; init; }
    public AudienceIdentifier Audience { get; init; }
    public ScopeCollection Scopes { get; init; }
    public Pkce? Pkce { get; init; }
    public string? Nonce { get; init; }
    
    private AuthorizeCommand(
        string responseType,
        ClientId clientId,
        string subject,
        RedirectUri redirectUri,
        AudienceIdentifier audience,
        ScopeCollection scopes,
        Pkce? pkce,
        string? nonce)
    {
        ResponseType = responseType;
        ClientId = clientId;
        Subject = subject;
        RedirectUri = redirectUri;
        Audience = audience;
        Scopes = scopes;
        Pkce = pkce;
        Nonce = nonce;
    }

    public static AuthorizeCommand Create(
        string responseType,
        ClientId clientId,
        string subject,
        RedirectUri redirectUri,
        AudienceIdentifier audience,
        ScopeCollection scopes,
        Pkce? pkce,
        string? nonce) =>
        new AuthorizeCommand(responseType, clientId, subject, redirectUri, audience, scopes, pkce, nonce);
}