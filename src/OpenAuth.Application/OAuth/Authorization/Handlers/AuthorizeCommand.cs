using OpenAuth.Application.Exceptions;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public record AuthorizeCommand
{
    public string ResponseType { get; init; }
    public ClientId ClientId { get; init; }
    public string Subject { get; init; }
    public RedirectUri RedirectUri { get; init; }
    public ScopeCollection Scopes { get; init; }
    public Pkce? Pkce { get; init; }
    public string? Nonce { get; init; }
    
    private AuthorizeCommand(
        string responseType,
        ClientId clientId,
        string subject,
        RedirectUri redirectUri,
        ScopeCollection scopes,
        Pkce? pkce,
        string? nonce)
    {
        ResponseType = responseType;
        ClientId = clientId;
        Subject = subject;
        RedirectUri = redirectUri;
        Scopes = scopes;
        Pkce = pkce;
    }

    public static AuthorizeCommand Create(
        string responseType,
        string clientId,
        string subject,
        string redirectUri,
        string scopes,
        string? codeChallenge,
        string? codeChallengeMethod,
        string? nonce)
    {
        if (!ClientId.TryCreate(clientId, out var id))
            throw new MalformedClientException("Invalid client_id parameter.");
        
        if (!RedirectUri.TryCreate(redirectUri, out var uri))
            throw new MalformedRedirectUriException("Invalid redirect_uri parameter.");

        if (!ScopeCollection.TryParse(scopes, out var scope))
            throw new MalformedScopeException("Invalid scope parameter.");
        
        if (!Pkce.TryParse(codeChallenge, codeChallengeMethod, out var pkce))
            throw new MalformedPkceException("Invalid PKCE parameters.");

        return new AuthorizeCommand(responseType, id, subject, uri, scope, pkce, nonce);
    }
}