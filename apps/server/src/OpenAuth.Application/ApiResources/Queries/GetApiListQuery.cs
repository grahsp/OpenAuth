using OpenAuth.Application.Abstractions;

namespace OpenAuth.Application.ApiResources.Queries;

public sealed record GetApiListQuery : IQuery<IReadOnlyList<ApiView>>;