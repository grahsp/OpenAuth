using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public sealed class AuthorizeCommandBuilder
{
	private string? _responseType;
	private ClientId? _clientId;
	private string? _subject;
	private RedirectUri? _redirectUri;
	private AudienceIdentifier? _audience;
	private ScopeCollection? _scope;
	private Pkce? _pkce;
	private string? _nonce;

	public AuthorizeCommandBuilder WithResponseType(string? responseType)
	{
		_responseType = responseType;
		return this;
	}
    
	public AuthorizeCommandBuilder WithClientId(Guid? clientId)
	{
		_clientId = clientId is null
			? null
			: ClientId.Create(clientId.ToString());
		
		return this;
	}

	public AuthorizeCommandBuilder WithSubject(string? subject)
	{
		_subject = subject;
		return this;
	}
    
	public AuthorizeCommandBuilder WithRedirectUri(string? redirectUri)
	{
		_redirectUri = redirectUri is null
			? null
			: RedirectUri.Create(redirectUri);
		
		return this;
	}
    
	public AuthorizeCommandBuilder WithAudience(string? audience)
	{
		_audience = audience is null
			? null
			: AudienceIdentifier.Create(audience);
		
		return this;
	}
    
	public AuthorizeCommandBuilder WithScope(string? scope)
	{
		_scope = scope is null
			? null
			: ScopeCollection.Parse(scope);
		
		return this;
	}
    
	public AuthorizeCommandBuilder WithPkce(Pkce? pkce)
	{
		_pkce = pkce;
		return this;
	}
    
	public AuthorizeCommandBuilder WithNonce(string? nonce)
	{
		_nonce = nonce;
		return this;
	}

	public AuthorizeCommand Build()
	{
		var responseType = _responseType ?? DefaultValues.ResponseType;
		var clientId = _clientId ?? ClientId.Create(DefaultValues.ClientId);
		var subject = _subject ?? DefaultValues.Subject;
		var redirectUri = _redirectUri ?? RedirectUri.Create(DefaultValues.RedirectUri);
		var scope = _scope ?? ScopeCollection.Parse(DefaultValues.Scopes);
		var audience = _audience ?? AudienceIdentifier.Create(DefaultValues.Audience);
		var pkce = _pkce ?? Pkce.FromVerifier(DefaultValues.CodeVerifier, DefaultValues.CodeChallengeMethod);
		var nonce = _nonce ?? DefaultValues.Nonce;

		return AuthorizeCommand.Create(
			responseType,
			clientId,
			subject,
			redirectUri,
			audience,
			scope,
			pkce,
			nonce);
	}
}
