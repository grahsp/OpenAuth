namespace OpenAuth.Server.Secrets;

public sealed record CreatedSecretResponse(
	string Id,
	string PlainTextSecret,
	DateTimeOffset CreatedAt,
	DateTimeOffset ExpiresAt
);