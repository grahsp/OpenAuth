using OpenAuth.Application.Shared;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients;

public sealed class ClientNotFoundException(ClientId clientId) : NotFoundException($"Client with ID '{clientId}' not found.");