using System.Text.Json.Serialization;

namespace OpenAuth.Api.Identity.Token;

public sealed record TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; }
    
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; }
    
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken{ get; }
    
    [JsonPropertyName("id_token")]
    public string? IdToken{ get; }
    
    
    // Errors
    [JsonPropertyName("error")]
    public string? Error { get; }
    
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; }


    public TokenResponse(
        string? accessToken,
        string? tokenType,
        int? expiresIn,
        string? idToken,
        string? refreshToken,
        string? scope,
        string? error,
        string? errorDescription)
    {
        AccessToken = accessToken;
        TokenType = tokenType;
        ExpiresIn = expiresIn;
        IdToken = idToken;
        RefreshToken = refreshToken;
        Scope = scope;

        Error = error;
        ErrorDescription = errorDescription;
    }
    public static TokenResponse Success(string accessToken, string tokenType, int expiresIn,
        string? idToken = null, string? refreshToken = null, string? scope = null)
    {
        return new TokenResponse
        (
            accessToken: accessToken,
            tokenType: tokenType,
            expiresIn: expiresIn,
            idToken: idToken,
            refreshToken: refreshToken,
            scope: scope,
            error: null,
            errorDescription: null
        );
    }

    public static TokenResponse Fail(string error, string? errorDescription = null)
    {
        return new TokenResponse
        (
            accessToken: null,
            tokenType: null,
            expiresIn: null,
            idToken: null,
            refreshToken: null,
            scope: null,
            error: error,
            errorDescription:
            errorDescription
        );
    }
}