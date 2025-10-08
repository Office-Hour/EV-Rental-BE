namespace Application.DTOs;

public sealed record PagingDto(int Page = 1, int PageSize = 20);
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);