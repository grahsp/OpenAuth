using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Queries.GetClientDetails;

public sealed record GetClientDetailsQuery(ClientId Id) : IQuery<ClientDetails?>;