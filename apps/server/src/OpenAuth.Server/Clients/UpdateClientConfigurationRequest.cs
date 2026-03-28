namespace OpenAuth.AuthorizationApi.Clients;

public sealed record UpdateClientConfigurationRequest(
	string Name,
	string ApplicationType,
	string[] RedirectUris,
	int TokenLifetimeInSeconds,
	string[] AllowedGrantTypes
);