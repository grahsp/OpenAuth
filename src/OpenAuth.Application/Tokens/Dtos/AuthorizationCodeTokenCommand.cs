using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record AuthorizationCodeTokenCommand : TokenCommand
{
    public override GrantType GrantType => GrantType.AuthorizationCode;
    
    public string Code { get; init; }
    public RedirectUri RedirectUri { get; init; }
    
    public string? CodeVerifier { get; init; }
    public string? ClientSecret { get; init; }

    private AuthorizationCodeTokenCommand(
        string code,
        ClientId clientId,
        RedirectUri redirectUri,
        ScopeCollection? scope,
        string? codeVerifier,
        string? clientSecret)
        : base (clientId, scope)
    {
        Code = code;
        RedirectUri = redirectUri;
        CodeVerifier = codeVerifier;
        ClientSecret = clientSecret;
    }

    public static AuthorizationCodeTokenCommand Create(
        string code,
        ClientId clientId,
        RedirectUri redirectUri,
        ScopeCollection? scope,
        string? codeVerifier,
        string? clientSecret)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Code is required.");

        if (string.IsNullOrWhiteSpace(codeVerifier) && string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Either PKCE verifier or client secret must be present.");

        return new AuthorizationCodeTokenCommand(
            code,
            clientId,
            redirectUri,
            scope,
            codeVerifier,
            clientSecret);
    }
}