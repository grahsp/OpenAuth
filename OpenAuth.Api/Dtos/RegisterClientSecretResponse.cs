namespace OpenAuth.Api.Dtos;

public record RegisterClientSecretResponse(
    ClientSecretResponse Secret,
    string Plain
);