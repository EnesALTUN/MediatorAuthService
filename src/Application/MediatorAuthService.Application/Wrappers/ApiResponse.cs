namespace MediatorAuthService.Application.Wrappers;

public class ApiResponse<TData>
{
    public TData? Data { get; set; }

    public List<string>? Errors { get; set; }

    public bool IsSuccessful { get; set; }

    public int StatusCode { get; set; }

    public int TotalItemCount { get; set; }
}