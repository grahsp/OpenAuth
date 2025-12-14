namespace OpenAuth.Api.Management.Contracts;

public sealed record PagedRequestDto(int Page = 1, int PageSize = 20);