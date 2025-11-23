namespace OpenAuth.Api.Dtos;

public record CreatedSecretResponse(
    SecretResponse Secret,
    string Plain
);