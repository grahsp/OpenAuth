namespace OpenAuth.Api.Dtos;

public record ClientCredentialsRequest (
    string ClientId,
    string ClientSecret,
    string GrantType,
    string Audience,
    string[] Scopes
);