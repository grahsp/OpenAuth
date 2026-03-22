using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.ApiResources.ValueObjects;

namespace OpenAuth.Application.ApiResources.Queries;

public sealed record GetApiQuery(ApiResourceId Id) : IQuery<ApiView?>;