namespace OpenAuth.Server.Management.Clients;

public sealed record UpdateClientConfigurationRequest(
	string Name,
	string ApplicationType,
	string[] RedirectUris,
	int TokenLifetimeInSeconds,
	string[] AllowedGrantTypes
);