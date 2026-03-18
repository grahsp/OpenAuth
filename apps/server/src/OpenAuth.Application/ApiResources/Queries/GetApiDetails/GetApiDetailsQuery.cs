using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.ApiResources.ValueObjects;

namespace OpenAuth.Application.ApiResources.Queries.GetApiDetails;

public sealed record GetApiDetailsQuery(ApiResourceId Id) : IQuery<ApiDetailsView>;