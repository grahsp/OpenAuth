using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class AuthorizationGrantBuilder
{
    private GrantType _grantType = GrantType.AuthorizationCode;
    private ClientId _clientId = ClientId.Create(DefaultValues.ClientId);
    private string _subject = DefaultValues.Subject;
    private RedirectUri _redirectUri = RedirectUri.Create(DefaultValues.RedirectUri);
    private ScopeCollection _scopes = ScopeCollection.Parse(DefaultValues.Scopes);

    private string _code = DefaultValues.Code;
    private Pkce? _pkce = TestData.CreateValidPkce();

    private string? _nonce = DefaultValues.Nonce;

    private DateTimeOffset _utcNow = DateTime.UtcNow;

    public AuthorizationGrantBuilder WithGrantType(GrantType grantType)
    {
        _grantType = grantType;
        return this;
    }

    public AuthorizationGrantBuilder WithClientId(ClientId clientId)
    {
        _clientId = clientId;
        return this;
    }

    public AuthorizationGrantBuilder WithSubject(string subject)
    {
        _subject = subject;
        return this;
    }

    public AuthorizationGrantBuilder WithRedirectUri(string redirectUri)
    {
        _redirectUri = RedirectUri.Create(redirectUri);
        return this;
    }

    public AuthorizationGrantBuilder WithScopes(string scopes)
    {
        _scopes = ScopeCollection.Parse(scopes);
        return this;
    }

    public AuthorizationGrantBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public AuthorizationGrantBuilder WithPkce(Pkce? pkce)
    {
        _pkce = pkce;
        return this;
    }

    public AuthorizationGrantBuilder WithNonce(string? nonce)
    {
        _nonce = nonce;
        return this;
    }

    public AuthorizationGrantBuilder WithCreationDate(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
        return this;
    }

    public AuthorizationGrant Build()
        => AuthorizationGrant.Create(
            _code,
            _grantType,
            _subject,
            _clientId,
            _redirectUri,
            _scopes,
            _pkce,
            _nonce,
            _utcNow
        );
}