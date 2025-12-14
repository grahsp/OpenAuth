namespace OpenAuth.Api.Management.Contracts;

public record PagedResponseDto<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);