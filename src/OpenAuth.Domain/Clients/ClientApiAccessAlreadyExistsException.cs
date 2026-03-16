using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients;

public sealed class ClientApiAccessAlreadyExistsException(ClientId clientId, ApiResourceId apiId) : Exception($"Client '{clientId}' already has access to API resource '{apiId}'.");