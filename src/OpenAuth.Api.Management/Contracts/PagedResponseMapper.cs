using OpenAuth.Application.Shared.Models;

namespace OpenAuth.Api.Management.Contracts;

public static class PagedResponseMapper
{
    public static PagedResponseDto<TDto> ToResponse<T, TDto>(
        this PagedResult<T> result,Func<T, TDto> map)
        => new(result.Items.Select(map), result.TotalCount, result.Page, result.PageSize);
}