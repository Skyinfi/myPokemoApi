namespace MyPokemoApi.Models.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}

public class ApiErrorResponse
{
    public ApiError Error { get; set; } = new();
}

public class ApiError
{
    public int? InternalCode { get; set; }
    public string? Message { get; set; }
}