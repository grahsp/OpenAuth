namespace OpenAuth.AuthorizationApi.Secrets;

public sealed record SecretResponse(
	string Id,
	DateTimeOffset CreatedAt,
	DateTimeOffset ExpiresAt,
	DateTimeOffset? RevokedAt
);