namespace OpenAuth.Application.Clients.Queries.GetClientDetails;

public sealed record ClientDetails(
	Guid Id,
	string Name,
	string ApplicationType,
	IReadOnlyList<string> RedirectUris,
	TimeSpan TokenLifetime,
	IReadOnlyList<string> AllowedGrantTypes
);