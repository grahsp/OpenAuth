using OpenAuth.Application.Secrets.Dtos;
using OpenAuth.Domain.Services.Dtos;

namespace OpenAuth.ManagementApi.Secrets;

public static class SecretMapper
{
	public static SecretResponse ToResponse(this SecretInfo secret)
	{
		return new SecretResponse(
			secret.Id.ToString(),
			secret.CreatedAt,
			secret.ExpiresAt,
			secret.RevokedAt
		);
	}

	public static CreatedSecretResponse ToResponse(this SecretCreationResult secret)
	{
		return new CreatedSecretResponse(
			secret.SecretId.ToString(),
			secret.PlainTextSecret,
			secret.CreatedAt,
			secret.ExpiresAt
		);
	}
}