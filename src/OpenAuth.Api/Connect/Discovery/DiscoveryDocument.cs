using System.Text.Json.Serialization;

namespace OpenAuth.Api.Identity.Discovery;

public record DiscoveryDocument
{
    [JsonPropertyName("issuer")]
    public string Issuer { get; init; }
    
    [JsonPropertyName("authorization_endpoint")]
    public string AuthorizationEndpoint { get; init; }
    
    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint { get; init; }
    
    [JsonPropertyName("userinfo_endpoint")] 
    public string UserInfoEndpoint { get; init; }
    
    [JsonPropertyName("jwks_uri")] 
    public string JwksUri { get; init; }
    
    [JsonPropertyName("response_types_supported")]
    public IEnumerable<string> ResponseTypesSupported { get; init; }
    
    [JsonPropertyName("subject_types_supported")]
    public IEnumerable<string> SubjectTypesSupported{ get; init; }
    
    [JsonPropertyName("id_token_signing_alg_values_supported")]
    public IEnumerable<string> IdTokenSigningAlgValuesSupported { get; init; }
    
    [JsonPropertyName("scopes_supported")]
    public IEnumerable<string> ScopesSupported { get; init; }
    
    [JsonPropertyName("claims_supported")]
    public IEnumerable<string> ClaimsSupported { get; init; }

    public DiscoveryDocument(
        string issuer,
        string authorization,
        string token,
        string user,
        string jwks,
        IEnumerable<string> responseTypes,
        IEnumerable<string> subjectTypes,
        IEnumerable<string> signingAlgValues,
        IEnumerable<string> scopes,
        IEnumerable<string> claims)
    {
        Issuer = issuer;
        AuthorizationEndpoint = authorization;
        TokenEndpoint = token;
        UserInfoEndpoint = user;
        
        JwksUri = jwks;
        
        ResponseTypesSupported = responseTypes;
        SubjectTypesSupported = subjectTypes;
        IdTokenSigningAlgValuesSupported = signingAlgValues;
        ScopesSupported = scopes;
        ClaimsSupported = claims;
    }
   
}