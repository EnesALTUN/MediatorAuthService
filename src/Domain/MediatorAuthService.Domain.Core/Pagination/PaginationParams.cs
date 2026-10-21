namespace MediatorAuthService.Domain.Core.Pagination;

public class PaginationParams
{
    public const int MaxPageSize = 100;

    public int PageId { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? OrderKey { get; set; }

    public string? OrderType { get; set; }
}
