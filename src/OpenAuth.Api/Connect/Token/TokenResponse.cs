using System.Text.Json.Serialization;

namespace OpenAuth.Api.Connect.Token;

public record TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }
    
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; init; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }
    
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken{ get; init; }
    
    
    // Errors
    [JsonPropertyName("error")]
    public string? Error { get; init; }
    
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }
    

    public static TokenResponse Success(string accessToken, string tokenType, int expiresIn,
        string? refreshToken = null, string? scope = null)
    {
        return new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = tokenType,
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken,
            Scope = scope
        };
    }

    public static TokenResponse Fail(string error, string? errorDescription = null)
    {
        return new TokenResponse
        {
            Error = error,
            ErrorDescription = errorDescription
        };
    }
}