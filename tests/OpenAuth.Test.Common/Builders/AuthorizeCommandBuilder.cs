using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public sealed class AuthorizeCommandBuilder
{
	private string _responseType = DefaultValues.ResponseType;
	private ClientId _clientId = ClientId.Parse(DefaultValues.ClientId);
	private string _subject = DefaultValues.Subject;
	private string _redirectUri = DefaultValues.RedirectUri;
	private string _audience = DefaultValues.ApiAudience;
	private string _scope = DefaultValues.Scopes;
	private Pkce? _pkce;
	private string? _nonce;

	public AuthorizeCommandBuilder WithResponseType(string responseType)
	{
		_responseType = responseType;
		return this;
	}
    
	public AuthorizeCommandBuilder WithClientId(ClientId clientId)
	{
		_clientId = clientId;
		return this;
	}

	public AuthorizeCommandBuilder WithSubject(string subject)
	{
		_subject = subject;
		return this;
	}
    
	public AuthorizeCommandBuilder WithRedirectUri(string redirectUri)
	{
		_redirectUri = redirectUri;
		return this;
	}
    
	public AuthorizeCommandBuilder WithAudience(string audience)
	{
		_audience = audience;
		return this;
	}
    
	public AuthorizeCommandBuilder WithScope(string scope)
	{
		_scope = scope;
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
		var redirectUri = RedirectUri.Parse(_redirectUri);
		var audience = new AudienceIdentifier(_audience);
		var scope = ScopeCollection.Parse(_scope);

		return AuthorizeCommand.Create(
			_responseType,
			_clientId,
			_subject,
			redirectUri,
			audience,
			scope,
			_pkce,
			_nonce);
	}
}
