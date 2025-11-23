namespace OpenAuth.Test.Integration.Infrastructure.Builders;

public class TokenRequestBuilder
{
    private readonly Dictionary<string, string?> _values = [];

    private TokenRequestBuilder WithKeyValue(string key, string? value)
    {
        _values[key] = value;
        return this;
    }
    
    public TokenRequestBuilder WithGrantType(string grantType)
        => WithKeyValue("grant_type", grantType);

    public TokenRequestBuilder WithClientId(string clientId)
        => WithKeyValue("client_id", clientId);
    
    public TokenRequestBuilder WithRedirectUri(string? redirectUri)
        => WithKeyValue("redirect_uri", redirectUri);
    
    public TokenRequestBuilder WithAudience(string? audience)
        => WithKeyValue("audience", audience);
    
    public TokenRequestBuilder WithScopes(string? scope)
        => WithKeyValue("scope", scope);
    
    public TokenRequestBuilder WithCode(string? code)
        => WithKeyValue("code", code);
    
    public  TokenRequestBuilder WithCodeVerifier(string? codeVerifier)
        => WithKeyValue("code_verifier", codeVerifier);
    
    public TokenRequestBuilder WithClientSecret(string? clientSecret)
        => WithKeyValue("client_secret", clientSecret);
    
    public Dictionary<string, string?> Build()
    {
        return _values;
    }
}