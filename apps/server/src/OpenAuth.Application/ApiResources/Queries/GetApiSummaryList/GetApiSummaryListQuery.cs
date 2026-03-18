using OpenAuth.Application.Abstractions;

namespace OpenAuth.Application.ApiResources.Queries.GetApiSummaryList;

public sealed record GetApiSummaryListQuery : IQuery<IReadOnlyList<ApiSummaryView>>;