using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Queries.GetClientApiScopes;

public sealed record GetClientApiScopesQuery(
	ClientId ClientId,
	AudienceIdentifier Audience
) : IQuery<ScopeCollection>;