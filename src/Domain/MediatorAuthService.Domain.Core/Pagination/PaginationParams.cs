namespace MediatorAuthService.Domain.Core.Pagination;

public class PaginationParams
{
    public int PageId { get; set; }

    public int PageSize { get; set; }

    public string? OrderKey { get; set; }

    public string? OrderType { get; set; }
}