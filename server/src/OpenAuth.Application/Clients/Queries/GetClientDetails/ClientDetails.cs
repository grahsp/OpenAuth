namespace OpenAuth.Application.Clients.Queries.GetClientDetails;

public sealed record ClientDetails(
	Guid Id,
	string Name,
	string ApplicationType,
	IReadOnlyList<string> RedirectUris,
	double TokenLifetimeInSeconds,
	IReadOnlyList<string> AllowedGrantTypes,
	IReadOnlyList<string> AvailableGrantTypes
);