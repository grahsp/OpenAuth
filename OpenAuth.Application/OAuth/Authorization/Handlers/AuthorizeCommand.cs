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
    
    private AuthorizeCommand(
        string responseType,
        ClientId clientId,
        string subject,
        RedirectUri redirectUri,
        ScopeCollection scopes,
        Pkce? pkce)
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
        string? codeChallengeMethod)
    {
        if (!ClientId.TryCreate(clientId, out var id))
            throw new InvalidClientException("Invalid client_id parameter.");
        
        if (!RedirectUri.TryCreate(redirectUri, out var uri))
            throw new InvalidRedirectUriException("Invalid redirect_uri parameter.");

        if (!ScopeCollection.TryParse(scopes, out var scope))
            throw new InvalidRequestException("Invalid or malformed scope parameter.");
        
        if (!Pkce.TryParse(codeChallenge, codeChallengeMethod, out var pkce))
            throw new InvalidRequestException("Invalid PKCE parameters.");

        return new AuthorizeCommand(responseType, id, subject, uri, scope, pkce);
    }
}